using backend.Data;
using backend.DTOs;
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

            // Get a list of cards that have already been successfully played by the team in this game.
            var playedCardIds = await _context.GameLogs
                .AsNoTracking()
                .Where(gl => gl.Games_Id == gameId && gl.Teams_Id == teamId && gl.Status == true)
                .Select(gl => gl.Cards_Id)
                .Distinct()
                .ToListAsync();

            // Create a lookup for card enablers (dependencies).
            var enablersMap = await _context.CardEnablers
                .AsNoTracking()
                .Where(de => de.Enablers_Id.HasValue && !playedCardIds.Contains(de.Enablers_Id.Value))
                .GroupBy(de => de.Cards_Id)
                .ToDictionaryAsync(g => g.Key, g => g.Select(de => de.Enablers_Id.Value).ToList());

            // Fetch Decision cards, filtering out any that have already been played.
            var decisionCards = await _context.Decisions
                .AsNoTracking()
                .Include(d => d.Card)
                .Where(d => d.Card.Decks_Id == deckId && !playedCardIds.Contains(d.Cards_Id))
                .OrderBy(d => d.Card.Card_Id)
                .Select(d => new UnifiedCardDto
                {
                    // CORRECTED: Use the public Card_Id instead of the primary key.
                    Id = d.Card.Card_Id,
                    DeckId = deckId,
                    Title = d.Decisions_Short_Desc,
                    Description = d.Decisions_Long_Desc,
                    CardType = "Decision",
                    Cost = d.Decisions_Cost_Bits,
                    Enablers = enablersMap.ContainsKey(d.Cards_Id) ? enablersMap[d.Cards_Id] : new List<int>()
                }).ToListAsync();

            // Fetch Hardware cards, treating them as items.
            var hardwareCards = await _context.Hardwares
                .AsNoTracking()
                .Include(h => h.Cards)
                .Where(h => h.Cards.Decks_Id == deckId && !playedCardIds.Contains(h.Cards_Id))
                .Select(h => new UnifiedCardDto
                {
                    // CORRECTED: Use the public Card_Id.
                    Id = h.Cards.Card_Id,
                    DeckId = deckId,
                    Title = h.Hardwares_Short_Desc,
                    Description = h.Hardwares_Long_Desc,
                    CardType = "Item",
                    Cost = h.Hardwares_Cost_Bits,
                    Enablers = new List<int>()
                }).ToListAsync();

            // NEW: Fetch Software cards, also treating them as items.
            var softwareCards = await _context.Softwares
                .AsNoTracking()
                .Include(s => s.Cards)
                .Where(s => s.Cards.Decks_Id == deckId && !playedCardIds.Contains(s.Cards_Id))
                .Select(s => new UnifiedCardDto
                {
                    // CORRECTED: Use the public Card_Id.
                    Id = s.Cards.Card_Id,
                    DeckId = deckId,
                    Title = s.Softwares_Short_Desc,
                    Description = s.Softwares_Long_Desc,
                    CardType = "Item",
                    Cost = s.Softwares_Cost_Bits,
                    Enablers = new List<int>()
                }).ToListAsync();

            // NEW: Combine Hardware and Software cards into a single list of items and sort them.
            var itemCards = hardwareCards.Concat(softwareCards)
                                         .OrderBy(i => i.Id)
                                         .ToList();

            return new CategorizedCardsDto { DecisionCards = decisionCards, ItemCards = itemCards };
        }

        public async Task<object> GetPlayerSessionDataAsync(string teamToken)
        {
            var team = await _context.Teams
                .AsNoTracking()
                .Include(t => t.Games).ThenInclude(g => g.Decks)
                .Include(t => t.Games).ThenInclude(g => g.Teams_Boards)
                .Include(t => t.Games).ThenInclude(g => g.Rivals_Boards)
                .FirstOrDefaultAsync(t => t.Teams_Token == teamToken);

            if (team == null) throw new Exception("Nie znaleziono drużyny dla podanego tokena.");
            if (team.Games == null) throw new Exception("Błąd danych: Drużyna nie jest przypisana do gry.");
            if (team.Games.Game_Status == GameStatus.End) throw new Exception("Ta gra została już zakończona.");
            if (team.Games.Teams_Boards == null) throw new Exception("Błąd konfiguracji gry: Brak danych planszy.");

            return new
            {
                gameId = team.Games.Games_Id,
                gameName = team.Games.Games_Desc,
                teamId = team.Teams_Id,
                teamName = team.Teams_Name,
                deckId = team.Games.Decks_Id,
                boardConfig = new
                {
                    boardId = team.Games.Teams_Boards.Boards_Id,
                    Name = team.Games.Teams_Boards.Name,
                    LabelsUp = team.Games.Teams_Boards.Labels_Up?.Split(';'),
                    LabelsRight = team.Games.Teams_Boards.Labels_Right?.Split(';'),
                    team.Games.Teams_Boards.Description_Down,
                    team.Games.Teams_Boards.Description_Left,
                    team.Games.Teams_Boards.Rows,
                    team.Games.Teams_Boards.Cols
                },
                rivalBoardConfig = team.Games.Rivals_Boards != null ? new
                {
                    boardId = team.Games.Rivals_Boards.Boards_Id,
                    Name = team.Games.Rivals_Boards.Name,
                    LabelsUp = team.Games.Rivals_Boards.Labels_Up?.Split(';'),
                    LabelsRight = team.Games.Rivals_Boards.Labels_Right?.Split(';'),
                    team.Games.Rivals_Boards.Description_Down,
                    team.Games.Rivals_Boards.Description_Left,
                    team.Games.Rivals_Boards.Rows,
                    team.Games.Rivals_Boards.Cols
                } : null
            };
        }

        public async Task<IEnumerable<object>> GetProcessPawnsForBoardAsync(int gameId, int teamId, int boardId)
        {
            return await _context.GameBoards
                .AsNoTracking()
                .Where(gb => gb.Games_Id == gameId && gb.Teams_Id == teamId && gb.Boards_Id == boardId && gb.Games_Processes_Id != null)
                .Select(gb => new
                {
                    GPId = gb.Games_Processes_Id,
                    PosX = gb.Poz_X,
                    PosY = gb.Poz_Y,
                    Color = gb.Games_Processes.Processes.Processes_Color,
                    Name = gb.Games_Processes.Processes.Processes_Desc
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