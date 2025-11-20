using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Dtos;

namespace backend.Services
{
    public interface ICheatsheetService
    {
        Task<CheatsheetMapDto> GetEnablersMapAsync(int deckId, int? moduleId);
        Task<object> GetLatestEntriesAsync(int gameId, int? teamId);
    }

    public class CheatsheetService : ICheatsheetService
    {
        private readonly AppDbContext _context;

        public CheatsheetService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CheatsheetMapDto> GetEnablersMapAsync(int deckId, int? moduleId)
        {
            var baseCardQuery = _context.Cards.AsNoTracking();

            if (moduleId.HasValue)
            {
                baseCardQuery = baseCardQuery.Where(card => card.Modules_Id == moduleId.Value);
            }
            else
            {
                baseCardQuery = baseCardQuery.Where(card => card.Decks_Id == deckId);
            }

            // --- POPRAWIONA SEKWENCJA WYKONYWANIA ZAPYTAŃ ---

            // Krok 1: Wykonaj PIERWSZE zapytanie i poczekaj na jego wynik.
            var cardTypes = await baseCardQuery
                .GroupJoin(_context.Decisions, c => c.Cards_Id, d => d.Cards_Id, (c, d) => new { c, d })
                .SelectMany(t => t.d.DefaultIfEmpty(), (t, d) => new { t.c, d })
                .GroupJoin(_context.Hardwares, p => p.c.Cards_Id, h => h.Cards_Id, (p, h) => new { p.c, p.d, h })
                .SelectMany(t => t.h.DefaultIfEmpty(), (t, h) => new { t.c, t.d, h })
                .GroupJoin(_context.Softwares, p => p.c.Cards_Id, s => s.Cards_Id, (p, s) => new { p.c, p.d, p.h, s })
                .SelectMany(t => t.s.DefaultIfEmpty(), (t, s) => new { t.c, t.d, t.h, s })
                .Select(result => new CardTypeDto
                {
                    Cards_Id = result.c.Cards_Id,
                    Card_Id = result.c.Card_Id,
                    CardType = result.d != null ? "Decision" :
                               result.h != null ? "Hardware" :
                               result.s != null ? "Software" :
                               "Generic"
                })
                .ToListAsync();

            // Krok 2: Dopiero gdy pierwsze zapytanie się zakończy, wykonaj DRUGIE.
            var enablersMap = await baseCardQuery
                .Include(card => card.DecisionEnablers)
                    .ThenInclude(ce => ce.Enablers)
                .ToDictionaryAsync(
                    card => card.Card_Id,
                    card => card.DecisionEnablers
                                .Where(de => de.Enablers != null)
                                .Select(de => de.Enablers!.Card_Id)
                                .ToList()
                );

            // Krok 3: Złóż wyniki w jeden obiekt DTO.
            var result = new CheatsheetMapDto
            {
                CardTypes = cardTypes,
                EnablersMap = enablersMap
            };

            return result;
        }

        public async Task<object> GetLatestEntriesAsync(int gameId, int? teamId)
        {
            // Scenariusz 1: Podano gameId i teamId
            if (teamId.HasValue)
            {
                var latestCardId = await _context.GameLogs
                    .Where(gl => gl.Games_Id == gameId && gl.Teams_Id == teamId.Value && gl.Status == true)
                    .OrderByDescending(gl => gl.Data)
                    .Select(gl => gl.Cards_Id)
                    .FirstOrDefaultAsync();

                // Serwis zwraca dane (lub 0/null), kontroler decyduje o kodzie odpowiedzi
                return latestCardId;
            }
            // Scenariusz 2: Podano tylko gameId
            else
            {
                var latestEntriesByTeam = await _context.GameLogs
                    .Where(gl => gl.Games_Id == gameId && gl.Status == true)
                    .GroupBy(gl => gl.Teams_Id)
                    .Select(group => new
                    {
                        TeamId = group.Key,
                        CardId = group.OrderByDescending(g => g.Data).First().Cards_Id
                    })
                    .ToListAsync();

                return latestEntriesByTeam;
            }
        }
    }
}