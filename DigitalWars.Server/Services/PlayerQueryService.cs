using backend.Data;
using backend.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services
{
    public interface IPlayerQueryService
    {
        Task<CategorizedCardsDto> GetCategorizedCardsForDeckAsync(int deckId, int gameId, int teamId);
        Task<object> GetPlayerSessionDataAsync(string teamToken);
        Task<IEnumerable<object>> GetProcessPawnsForBoardAsync(int gameId, int teamId, int boardId);
        Task<IEnumerable<object>> GetRivalPawnsForBoardAsync(int gameId, int boardId);
    }
    public class PlayerQueryService : IPlayerQueryService
    {
        private readonly AppDbContext _context;

        public PlayerQueryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CategorizedCardsDto> GetCategorizedCardsForDeckAsync(int deckId, int gameId, int teamId)
        {
            if (!await _context.Decks.AnyAsync(d => d.Decks_Id == deckId))
                throw new Exception($"Talia o ID {deckId} nie została znaleziona.");

            // Krok 1: Pobierz WEWNĘTRZNE ID kart, które zostały już pomyślnie zagrane przez drużynę.
            var playedCardInternalIds = await _context.GameLogs
                .AsNoTracking()
                .Where(gl => gl.Games_Id == gameId && gl.Teams_Id == teamId && gl.Status == true && gl.Cards_Id.HasValue)
                .Select(gl => gl.Cards_Id!.Value)
                .Distinct()
                .ToHashSetAsync();

            // Krok 2: Pobierz WEWNĘTRZNE ID kart, które zostały specjalnie odblokowane dla tej drużyny w tej grze.
            var teamSpecificUnlockedCardIds = await _context.CardEnablers
                .AsNoTracking()
                .Where(ce => ce.Games_Id == gameId && ce.Teams_Id == teamId)
                .Select(ce => ce.Cards_Id)
                .Distinct()
                .ToHashSetAsync();

            // Krok 3: Stwórz mapę GLOBALNYCH zależności (gdzie Games_Id jest null).
            var enablersMap = await _context.CardEnablers
                .AsNoTracking()
                .Where(ce => ce.Games_Id == null && ce.Enablers_Id.HasValue)
                .Include(ce => ce.Enablers)
                .Where(ce => ce.Enablers != null)
                .Where(ce => !playedCardInternalIds.Contains(ce.Enablers_Id!.Value))
                .GroupBy(ce => ce.Cards_Id)
                .Select(g => new
                {
                    CardId = g.Key,
                    RequiredPublicCardIds = g.Select(ce => ce.Enablers!.Card_Id).ToList()
                })
                .ToDictionaryAsync(x => x.CardId, x => x.RequiredPublicCardIds);

            // Krok 4: Pobierz karty Decyzji
            var decisionCards = await _context.Decisions
                .AsNoTracking()
                .Include(d => d.Card)
                .Where(d => d.Card.Decks_Id == deckId && !playedCardInternalIds.Contains(d.Cards_Id))
                .OrderBy(d => d.Card.Card_Id)
                .Select(d => new UnifiedCardDto
                {
                    Id = d.Card.Card_Id,
                    DeckId = deckId,
                    Title = d.Decisions_Short_Desc,
                    Description = d.Decisions_Long_Desc,
                    Cost = d.Decisions_Cost_Bits,
                    Enablers = teamSpecificUnlockedCardIds.Contains(d.Cards_Id)
                                ? new List<int>()
                                : (enablersMap.ContainsKey(d.Cards_Id) ? enablersMap[d.Cards_Id] : new List<int>())
                }).ToListAsync();

            // Krok 5: Pobierz karty Sprzętu (Hardware)
            var hardwareCards = await _context.Hardwares
                .AsNoTracking()
                .Include(h => h.Cards)
                .Where(h => h.Cards.Decks_Id == deckId && !playedCardInternalIds.Contains(h.Cards_Id))
                .OrderBy(h => h.Cards.Card_Id) // Dodano sortowanie dla spójności
                .Select(h => new UnifiedCardDto
                {
                    Id = h.Cards.Card_Id,
                    DeckId = deckId,
                    Title = h.Hardwares_Short_Desc,
                    Description = h.Hardwares_Long_Desc,
                    Cost = h.Hardwares_Cost_Bits,
                    Enablers = new List<int>()
                }).ToListAsync();

            // Krok 6: Pobierz karty Oprogramowania (Software)
            var softwareCards = await _context.Softwares
                .AsNoTracking()
                .Include(s => s.Cards)
                .Where(s => s.Cards.Decks_Id == deckId && !playedCardInternalIds.Contains(s.Cards_Id))
                .OrderBy(s => s.Cards.Card_Id) // Dodano sortowanie dla spójności
                .Select(s => new UnifiedCardDto
                {
                    Id = s.Cards.Card_Id,
                    DeckId = deckId,
                    Title = s.Softwares_Short_Desc,
                    Description = s.Softwares_Long_Desc,
                    Cost = s.Softwares_Cost_Bits,
                    Enablers = new List<int>()
                }).ToListAsync();

            // Krok 7: Zwróć obiekt DTO z trzema oddzielnymi listami
            return new CategorizedCardsDto
            {
                DecisionCards = decisionCards,
                SoftwareCards = softwareCards,
                HardwareCards = hardwareCards,

            };
        }
        public async Task<object> GetPlayerSessionDataAsync(string teamToken)
        {
            var team = await _context.Teams
        .AsNoTracking()
        .Include(t => t.Games)
        .FirstOrDefaultAsync(t => t.Teams_Token == teamToken);

            if (team == null) throw new Exception("Nie znaleziono drużyny dla podanego tokena.");
            if (team.Games == null) throw new Exception("Błąd danych: Drużyna nie jest przypisana do gry.");

            // --- KLUCZOWA ZMIANA: Zwracanie obiektów ErrorResponseDto zamiast rzucania wyjątków ---
            switch (team.Games.Game_Status)
            {
                case GameStatus.Paused:
                    return new ErrorResponseDto { ErrorCode = "GamePaused", Message = "Gra jest obecnie wstrzymana." };
                case GameStatus.End:
                    return new ErrorResponseDto { ErrorCode = "GameEnded", Message = "Ta gra została już zakończona." };
            }
            // --- KONIEC ZMIANY ---

            // Jeśli status jest OK, kontynuujemy pobieranie reszty danych
            team = await _context.Teams
                .AsNoTracking()
                .Include(t => t.Games).ThenInclude(g => g.Decks)
                .Include(t => t.Games).ThenInclude(g => g.Teams_Boards)
                .Include(t => t.Games).ThenInclude(g => g.Rivals_Boards)
                .FirstOrDefaultAsync(t => t.Teams_Token == teamToken);

            if (team!.Games.Teams_Boards == null) throw new Exception("Błąd konfiguracji gry: Brak danych planszy.");

            // Reszta kodu pozostaje bez zmian
            return new
            {
                gameId = team.Games.Games_Id,
                gameName = team.Games.Games_Desc,
                teamId = team.Teams_Id,
                teamName = team.Teams_Name,
                teamColor = team.Teams_Color,
                teamBudget = team.Teams_Bud,
                deckId = team.Games.Decks_Id,
                IsOnline = team.Games.Is_Online, // Upewnij się, że te pola są zwracane
                IsIndependent = team.Is_Independent, // Upewnij się, że te pola są zwracane
                boardConfig = new
                {
                    boardId = team.Games.Teams_Boards.Boards_Id,
                    Name = team.Games.Teams_Boards.Name,
                    LabelsUp = team.Games.Teams_Boards.Labels_Up?.Split(';'),
                    LabelsRight = team.Games.Teams_Boards.Labels_Right?.Split(';'),
                    Borders_Colors = team.Games.Teams_Boards.Borders_Colors?.Split(';'),
                    team.Games.Teams_Boards.Description_Down,
                    team.Games.Teams_Boards.Description_Left,
                    team.Games.Teams_Boards.Rows,
                    team.Games.Teams_Boards.Cols,
                    team.Games.Teams_Boards.Cell_Color,
                    team.Games.Teams_Boards.Border_Color
                },
                rivalBoardConfig = team.Games.Rivals_Boards != null ? new
                {
                    boardId = team.Games.Rivals_Boards.Boards_Id,
                    Name = team.Games.Rivals_Boards.Name,
                    LabelsUp = team.Games.Rivals_Boards.Labels_Up?.Split(';'),
                    LabelsRight = team.Games.Rivals_Boards.Labels_Right?.Split(';'),
                    Borders_Colors = team.Games.Rivals_Boards.Borders_Colors?.Split(';'),
                    team.Games.Rivals_Boards.Description_Down,
                    team.Games.Rivals_Boards.Description_Left,
                    team.Games.Rivals_Boards.Rows,
                    team.Games.Rivals_Boards.Cols,
                    team.Games.Rivals_Boards.Cell_Color,
                    team.Games.Rivals_Boards.Border_Color
                } : null
            };
        }

        public async Task<IEnumerable<object>> GetProcessPawnsForBoardAsync(int gameId, int teamId, int boardId)
        {
            return await _context.GameBoards
                .AsNoTracking()
                .Where(gb =>
                    gb.Games_Id == gameId &&
                    gb.Teams_Id == teamId &&
                    gb.Boards_Id == boardId &&
                    gb.Games_Processes_Id != null &&
                    gb.Games_Processes != null &&
                    gb.Games_Processes.Processes != null)
                .Select(gb => new
                {
                    GPId = gb.Games_Processes_Id,
                    PosX = gb.Poz_X,
                    PosY = gb.Poz_Y,
                    Color = gb.Games_Processes!.Processes!.Processes_Color,
                    Name = gb.Games_Processes!.Processes!.Processes_Desc
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetRivalPawnsForBoardAsync(int gameId, int boardId)
        {
            return await _context.GameBoards
                .AsNoTracking()
                .Where(gb => gb.Games_Id == gameId && gb.Boards_Id == boardId && gb.Games_Processes_Id == null)
                .Select(gb => new
                {
                    PosX = gb.Poz_X,
                    PosY = gb.Poz_Y,
                    TeamColor = gb.Teams.Teams_Color,
                    TeamId = gb.Teams.Teams_Id,
                    TeamName = gb.Teams.Teams_Name
                })
                .ToListAsync();
        }
    }
}