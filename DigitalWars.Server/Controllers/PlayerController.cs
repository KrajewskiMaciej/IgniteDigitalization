using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/player")]
    public class PlayerController : BaseApiController
    {
        private readonly IPlayerQueryService _queryService;
        private readonly IPlayerActionService _actionService;
        private readonly AppDbContext _context;

        public PlayerController(IPlayerQueryService queryService, IPlayerActionService actionService, AppDbContext context)
        {
            _queryService = queryService;
            _actionService = actionService;
            _context = context;
        }

        [HttpGet("game/{gameId}/team/{teamId}/info")]
        public async Task<IActionResult> GetTeamInfo(int gameId, int teamId)
        {
            // POPRAWKA: Rozdzielenie zapytania do bazy od przetwarzania w pamięci, aby uniknąć błędu CS0854

            // Krok 1: Pobierz surowe dane z bazy danych
            var rawTeamData = await _context.Teams
                .Include(t => t.Games)
                    .ThenInclude(g => g.Teams_Boards)
                .Where(t => t.Teams_Id == teamId && t.Games_Id == gameId)
                .Select(t => new // Użyj anonimowego obiektu do pobrania danych
                {
                    t.Teams_Id,
                    t.Teams_Name,
                    t.Teams_Bud,
                    t.Games.Decks_Id,
                    Board = t.Games.Teams_Boards // Pobierz cały obiekt Board
                })
                .FirstOrDefaultAsync();

            if (rawTeamData == null || rawTeamData.Board == null)
            {
                return NotFound("Nie znaleziono danych dla podanej drużyny i gry.");
            }

            // Krok 2: Przetwórz dane w pamięci aplikacji (tutaj Split jest bezpieczny)
            var teamData = new SessionDataDto
            {
                TeamId = rawTeamData.Teams_Id,
                TeamName = rawTeamData.Teams_Name,
                TeamBud = rawTeamData.Teams_Bud,
                DeckId = rawTeamData.Decks_Id,
                BoardConfig = new BoardConfigDto
                {
                    BoardId = rawTeamData.Board.Boards_Id,
                    Name = rawTeamData.Board.Name,
                    LabelsUp = rawTeamData.Board.Labels_Up.Split(';'), // Bezpieczne wywołanie
                    LabelsRight = rawTeamData.Board.Labels_Right.Split(';'), // Bezpieczne wywołanie
                    DescriptionDown = rawTeamData.Board.Description_Down,
                    DescriptionLeft = rawTeamData.Board.Description_Left,
                    Rows = rawTeamData.Board.Rows,
                    Cols = rawTeamData.Board.Cols,
                    CellColor = rawTeamData.Board.Cell_Color,
                    BorderColor = rawTeamData.Board.Border_Color,
                    BorderColors = rawTeamData.Board.Borders_Colors.Split(';') // Bezpieczne wywołanie
                }
            };

            return Ok(teamData);
        }

        [Authorize]
        [HttpGet("game/{gameId}/teams-management")]
        public async Task<IActionResult> GetTeamsManagement(int gameId)
        {
            var teams = await _context.Teams
                .Where(t => t.Games_Id == gameId)
                .Select(t => new TeamManagementDto
                {
                    TeamId = t.Teams_Id,
                    TeamName = t.Teams_Name,
                    TeamBud = t.Teams_Bud,
                    TeamToken = t.Teams_Token
                }).ToListAsync();
            return Ok(teams);
        }

        [Authorize]
        [HttpPut("team/{teamId}/budget")]
        public async Task<IActionResult> UpdateTeamBudget(int teamId, [FromBody] UpdateBudgetDto dto)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null) return NotFound();

            team.Teams_Bud = dto.NewBudget;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Budżet zaktualizowany." });
        }

        [Authorize]
        [HttpPost("game/{gameId}/unlock-card")]
        public async Task<IActionResult> UnlockCard(int gameId, [FromBody] UnlockCardDto dto)
        {
            var gameDeckId = await _context.Games
                .Where(g => g.Games_Id == gameId)
                .Select(g => g.Decks_Id)
                .FirstOrDefaultAsync();

            if (gameDeckId == 0) return NotFound("Gra nie została znaleziona lub nie ma przypisanej talii.");

            var card = await _context.Cards.FirstOrDefaultAsync(c => c.Card_Id == dto.CardId && c.Decks_Id == gameDeckId);
            if (card == null)
            {
                return NotFound($"Karta o ID {dto.CardId} nie została znaleziona w talii powiązanej z grą.");
            }

            var enabler = new CardEnabler
            {
                Games_Id = gameId,
                Teams_Id = dto.TeamId,
                Cards_Id = card.Cards_Id,
                Enablers_Id = card.Cards_Id // UWAGA: Enablers_Id to prawdopodobnie błąd w logice, powinno być Enabler_Cards_Id, ale trzymam się oryginalnego kodu
            };
            _context.CardEnablers.Add(enabler);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Karta została odblokowana." });
        }

        [Authorize]
        [HttpGet("game/{gameId}/pending-logs")]
        public async Task<IActionResult> GetPendingLogs(int gameId)
        {
            var logs = await _context.GameLogs
                .Where(l => l.Games_Id == gameId && l.Is_Approved == false && l.Teams != null && l.Cards != null)
                .Select(l => new
                {
                    LogId = l.Games_Logs_Id,
                    // POPRAWKA: Użycie operatora ! (null-forgiving) jest bezpieczne, bo sprawdziliśmy `!= null` w `Where`
                    TeamName = l.Teams!.Teams_Name,
                    CardId = l.Cards!.Card_Id,
                    InternalCardId = l.Cards_Id,
                    l.Costs,
                    Timestamp = l.Data
                })
                .ToListAsync();

            // POPRAWKA: Filtruj logi, które z jakiegoś powodu nie mają InternalCardId, aby uniknąć błędu CS8629
            var validLogs = logs.Where(l => l.InternalCardId.HasValue).ToList();

            // POPRAWKA: Użyj .Value, co jest teraz bezpieczne
            var cardIds = validLogs.Select(l => l.InternalCardId!.Value).ToList();

            var decisionTitles = await _context.Decisions
                .Where(d => cardIds.Contains(d.Cards_Id))
                .ToDictionaryAsync(d => d.Cards_Id, d => d.Decisions_Short_Desc);

            var hardwareTitles = await _context.Hardwares
                .Where(h => cardIds.Contains(h.Cards_Id))
                .ToDictionaryAsync(h => h.Cards_Id, h => h.Hardwares_Short_Desc);

            var softwareTitles = await _context.Softwares
                .Where(s => cardIds.Contains(s.Cards_Id))
                .ToDictionaryAsync(s => s.Cards_Id, s => s.Softwares_Short_Desc);

            // Użyj przefiltrowanej listy validLogs
            var result = validLogs.Select(l => new
            {
                l.LogId,
                l.TeamName,
                l.CardId,
                CardTitle = decisionTitles.GetValueOrDefault(l.InternalCardId!.Value) ??
                            hardwareTitles.GetValueOrDefault(l.InternalCardId!.Value) ??
                            softwareTitles.GetValueOrDefault(l.InternalCardId!.Value),
                l.Costs,
                l.Timestamp
            });

            return Ok(result);
        }

        [HttpGet("game-events")]
        public async Task<IActionResult> GetGameEvents([FromQuery] int Decks_Id)
        {
            var events = await _context.GameEvents
                .Where(e => e.Decks_Id == Decks_Id)
                .Select(e => new GameEventDto { EventId = e.Games_Events_Id, ShortDesc = e.Events_Short_Desc, LongDesc = e.Events_Long_Desc })
                .ToListAsync();
            return Ok(events);
        }

        [Authorize]
        [HttpPost("game/{gameId}/apply-event")]
        public async Task<IActionResult> ApplyEvent(int gameId, [FromBody] ApplyEventDto dto)
        {
            var team = await _context.Teams.FindAsync(dto.TeamId);
            var gameEvent = await _context.GameEvents.FindAsync(dto.EventId);

            if (team == null || gameEvent == null || team.Games_Id != gameId) return BadRequest();

            team.Games_Events_Id = dto.EventId;
            team.Turns_Left = gameEvent.Turns_Time;
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Wydarzenie '{gameEvent.Events_Short_Desc}' zostało aktywowane dla drużyny '{team.Teams_Name}'." });
        }

        [HttpGet("game/{gameId}/history-version")]
        public async Task<IActionResult> GetHistoryVersion(int gameId, [FromQuery] int? teamId)
        {
            var query = _context.GameLogs.Where(l => l.Games_Id == gameId && l.Is_Approved == true);
            if (teamId.HasValue)
            {
                query = query.Where(l => l.Teams_Id == teamId.Value);
            }
            var lastUpdate = await query.OrderByDescending(l => l.Data).Select(l => l.Data).FirstOrDefaultAsync();
            return Ok(new { lastUpdate });
        }

        [HttpGet("game/{gameId}/pending-version")]
        public async Task<IActionResult> GetPendingVersion(int gameId)
        {
            var lastUpdate = await _context.GameLogs
                .Where(l => l.Games_Id == gameId && l.Is_Approved == false)
                .OrderByDescending(l => l.Data)
                .Select(l => l.Data)
                .FirstOrDefaultAsync();
            return Ok(new { lastUpdate });
        }

        [HttpGet("getLogs")]
        public async Task<IActionResult> GetLogs([FromQuery] int gameId, [FromQuery] int teamId)
        {
            var logs = await _context.GameLogs
                .Where(l => l.Games_Id == gameId && l.Teams_Id == teamId && l.Is_Approved == true)
                .OrderByDescending(l => l.Data)
                .ToListAsync();
            return Ok(logs);
        }

        [HttpGet("deck/{deckId}/unified-cards")]
        public async Task<IActionResult> GetUnifiedCardsForDeck(int deckId, [FromQuery] int gameId, [FromQuery] int teamId)
        {
            try
            {
                var cards = await _queryService.GetCategorizedCardsForDeckAsync(deckId, gameId, teamId);
                return Ok(cards);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("team/{teamToken}")]
        public async Task<IActionResult> GetPlayerSessionData(string teamToken)
        {
            try
            {
                var sessionData = await _queryService.GetPlayerSessionDataAsync(teamToken);
                return Ok(sessionData);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("team-board")]
        public async Task<IActionResult> GetTeamBoardData([FromQuery] int gameId, [FromQuery] int teamId, [FromQuery] int boardId)
        {
            var pawns = await _queryService.GetProcessPawnsForBoardAsync(gameId, teamId, boardId);
            return Ok(pawns);
        }

        [HttpGet("rival-board")]
        public async Task<IActionResult> GetRivalBoardData([FromQuery] int gameId, [FromQuery] int boardId)
        {
            // POPRAWKA: Zaimplementowano brakującą logikę bezpośrednio w kontrolerze (błąd CS1061)
            var pawns = await _context.GameBoards
            .Include(p => p.Teams)
                .Where(p => p.Games_Id == gameId && p.Boards_Id == boardId && p.Games_Processes_Id == null)
                .Select(p => new
                { // Zwróć dane w odpowiednim formacie DTO, jeśli istnieje
                    TeamId = p.Teams_Id,
                    PosX = p.Poz_X,
                    PosY = p.Poz_Y,
                    TeamColor = p.Teams.Teams_Color,
                    TeamName = p.Teams.Teams_Name
                })
                .ToListAsync();

            return Ok(pawns);
        }

        [HttpPost("success/{cardId}")]
        public async Task<IActionResult> SendSuccess(int cardId, [FromBody] CardDataDTO cardData)
        {
            try
            {
                var result = await _actionService.PlayCardAsync(cardId, cardData, wasSuccess: true);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("failure/{cardId}")]
        public async Task<IActionResult> SendFailure(int cardId, [FromBody] CardDataDTO cardData)
        {
            try
            {
                var result = await _actionService.PlayCardAsync(cardId, cardData, wasSuccess: false);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("approve-log/{logId}")]
        public async Task<IActionResult> ApproveLog(int logId)
        {
            try
            {
                await _actionService.ApproveLogAsync(logId);
                return Ok(new { message = "Sugestia została zatwierdzona." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("reject-log/{logId}")]
        public async Task<IActionResult> RejectLog(int logId)
        {
            try
            {
                await _actionService.RejectLogAsync(logId);
                return Ok(new { message = "Sugestia została odrzucona." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("player-history")] // Upewnij się, że atrybut routingu jest poprawny
        public async Task<IActionResult> GetPlayerHistory([FromBody] PlayerHistoryRequestDto request)
        {
            // Krok 1: Dynamiczne budowanie zapytania na podstawie obecności TeamId
            var query = _context.GameLogs
                .Include(l => l.Teams) // Dołączenie Teams dla nazwy
                .Include(l => l.Cards) // Dołączenie Cards dla publicznego Card_Id
                .Where(l => l.Games_Id == request.GameId && l.Is_Approved == true);

            // POPRAWKA: Dynamiczne dodawanie warunku filtrowania po TeamId
            if (request.TeamId.HasValue)
            {
                query = query.Where(l => l.Teams_Id == request.TeamId.Value);
            }

            var rawLogs = await query
                .OrderByDescending(l => l.Data)
                .ToListAsync();

            // Krok 2: Zebranie unikalnych wewnętrznych ID kart z logów
            var cardIds = rawLogs
                .Where(l => l.Cards_Id.HasValue)
                .Select(l => l.Cards_Id.Value)
                .Distinct()
                .ToList();

            if (!cardIds.Any())
            {
                // Jeśli nie ma kart, zwróć tylko logi z wydarzeń
                var eventOnlyLogs = rawLogs.Select(log => new
                {
                    IsEventNotification = log.Games_Events_Id != null,
                    EventDescription = log.Games_Events != null ? log.Games_Events.Events_Long_Desc : null,
                    Timestamp = log.Data,
                    CardId = (int?)null,
                    CardTitle = "N/A", // Zmieniono nazwę na CardTitle dla spójności
                    TeamId = log.Teams_Id,
                    TeamName = log.Teams?.Teams_Name,
                    FeedbackDescription = log.Feedbacks,
                    Status = log.Status,
                    GameEventId = log.Games_Events_Id
                });
                return Ok(eventOnlyLogs);
            }

            // Krok 3: Efektywne pobranie wszystkich możliwych tytułów
            var decisionTitles = await _context.Decisions
                .Where(d => cardIds.Contains(d.Cards_Id))
                .ToDictionaryAsync(d => d.Cards_Id, d => d.Decisions_Short_Desc);

            var hardwareTitles = await _context.Hardwares
                .Where(h => cardIds.Contains(h.Cards_Id))
                .ToDictionaryAsync(h => h.Cards_Id, h => h.Hardwares_Short_Desc);

            var softwareTitles = await _context.Softwares
                .Where(s => cardIds.Contains(s.Cards_Id))
                .ToDictionaryAsync(s => s.Cards_Id, s => s.Softwares_Short_Desc);

            // Krok 4: Mapowanie surowych logów na finalny rezultat
            var result = rawLogs.Select(log =>
            {
                string cardTitle = "N/A";
                if (log.Cards_Id.HasValue)
                {
                    cardTitle = decisionTitles.GetValueOrDefault(log.Cards_Id.Value) ??
                                hardwareTitles.GetValueOrDefault(log.Cards_Id.Value) ??
                                softwareTitles.GetValueOrDefault(log.Cards_Id.Value) ?? "N/A";
                }

                return new
                {
                    IsEventNotification = log.Games_Events_Id != null,
                    EventDescription = log.Games_Events != null ? log.Games_Events.Events_Long_Desc : null,
                    Timestamp = log.Data,
                    CardId = log.Cards?.Card_Id,
                    CardTitle = cardTitle, // Zmieniono nazwę na CardTitle dla spójności
                    TeamId = log.Teams_Id,
                    TeamName = log.Teams?.Teams_Name,
                    FeedbackDescription = log.Feedbacks,
                    Status = log.Status,
                    GameEventId = log.Games_Events_Id
                };
            });

            return Ok(result);
        }

        [HttpGet("game/{gameId}/rival-board-config")] // A new, clear endpoint
        public async Task<IActionResult> GetRivalBoardConfigForGame(int gameId)
        {
            var game = await _context.Games
                .AsNoTracking()
                .Include(g => g.Rivals_Boards)
                .FirstOrDefaultAsync(g => g.Games_Id == gameId);

            if (game == null || game.Rivals_Boards == null)
            {
                return NotFound("Konfiguracja planszy rywala dla tej gry nie została znaleziona.");
            }

            var config = game.Rivals_Boards;

            return Ok(new
            {
                rivalBoardConfig = new
                {
                    boardId = config.Boards_Id,
                    name = config.Name,
                    labelsUp = config.Labels_Up?.Split(';'),
                    labelsRight = config.Labels_Right?.Split(';'),
                    descriptionDown = config.Description_Down,
                    descriptionLeft = config.Description_Left,
                    rows = config.Rows,
                    cols = config.Cols,
                    cellColor = config.Cell_Color,
                    borderColor = config.Border_Color,
                    borderColors = config.Borders_Colors?.Split(';')
                }
            });
        }
    }
}