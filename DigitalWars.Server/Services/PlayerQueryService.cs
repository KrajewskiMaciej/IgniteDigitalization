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

            var playedCardIds = await _context.GameLogs
                .AsNoTracking()
                .Where(gl => gl.Games_Id == gameId && gl.Teams_Id == teamId && gl.Status == true)
                .Select(gl => gl.Cards_Id)
                .Distinct()
                .ToListAsync();

            var enablersMap = await _context.CardEnablers
                .AsNoTracking()
                .Where(de => de.Enablers_Id.HasValue && !playedCardIds.Contains(de.Enablers_Id.Value))
                .GroupBy(de => de.Cards_Id)
                .ToDictionaryAsync(g => g.Key, g => g.Select(de => de.Enablers_Id.Value).ToList());

            var decisionCards = await _context.Decisions
                .AsNoTracking()
                .Include(d => d.Card)
                .Where(d => d.Card.Decks_Id == deckId && !playedCardIds.Contains(d.Cards_Id))
                .OrderBy(d => d.Cards_Id)
                .Select(d => new UnifiedCardDto
                {
                    Id = d.Cards_Id,
                    DeckId = deckId,
                    Title = d.Decisions_Short_Desc,
                    Description = d.Decisions_Long_Desc,
                    CardType = "Decision",
                    Cost = d.Decisions_Cost_Bits,
                    Enablers = enablersMap.ContainsKey(d.Cards_Id) ? enablersMap[d.Cards_Id] : new List<int>()
                }).ToListAsync();

            var itemCards = await _context.Hardwares
                .AsNoTracking()
                 .Include(d => d.Cards)
                .Where(i => i.Cards.Decks_Id == deckId && !playedCardIds.Contains(i.Cards_Id))
                .OrderBy(i => i.Cards_Id)
                .Select(i => new UnifiedCardDto
                {
                    Id = i.Cards_Id,
                    DeckId = deckId,
                    Title = i.Hardwares_Short_Desc,
                    Description = i.Hardwares_Long_Desc,
                    CardType = "Item",
                    Cost = i.Hardwares_Cost_Bits,
                    Enablers = new List<int>()
                }).ToListAsync();

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