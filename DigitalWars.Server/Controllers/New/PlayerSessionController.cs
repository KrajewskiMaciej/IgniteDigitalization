using DigitalWars.Server.Dtos;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using backend.Hubs;

namespace DigitalWars.Server.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/[controller]")]
    public class PlayerSessionController : BaseApiController
    {
        private readonly IPlayerQueryService _queryService;
        private readonly AppDbContext _context;
        private readonly IHubContext<GameHub> _hubContext;

        public PlayerSessionController(IPlayerQueryService queryService, AppDbContext context, IHubContext<GameHub> hubContext)
        {
            _queryService = queryService;
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet("info")]
        public async Task<IActionResult> GetTeamInfo([FromQuery] int gameId, [FromQuery] int teamId)
        {
            var rawTeamData = await _context.Teams
                .Include(t => t.Games)
                    .ThenInclude(g => g.Teams_Boards)
                .Where(t => t.Teams_Id == teamId && t.Games_Id == gameId)
                .Select(t => new
                {
                    t.Teams_Id,
                    t.Teams_Name,
                    t.Teams_Bud,
                    t.Teams_Color,
                    t.Games.Decks_Id,
                    Board = t.Games.Teams_Boards
                })
                .FirstOrDefaultAsync();

            if (rawTeamData == null || rawTeamData.Board == null)
            {
                return NotFound(CreateError("DATA_NOT_FOUND", "dataNotFound"));
            }

            var teamData = new SessionDataDto
            {
                TeamId = rawTeamData.Teams_Id,
                TeamName = rawTeamData.Teams_Name,
                TeamColor = rawTeamData.Teams_Color,
                TeamBud = rawTeamData.Teams_Bud,
                DeckId = rawTeamData.Decks_Id,
                BoardConfig = new BoardConfigDto
                {
                    BoardId = rawTeamData.Board.Boards_Id,
                    Name = rawTeamData.Board.Name,
                    LabelsUp = rawTeamData.Board.Labels_Up?.Split(';'),
                    LabelsRight = rawTeamData.Board.Labels_Right?.Split(';'),
                    DescriptionDown = rawTeamData.Board.Description_Down,
                    DescriptionLeft = rawTeamData.Board.Description_Left,
                    Rows = rawTeamData.Board.Rows,
                    Cols = rawTeamData.Board.Cols,
                    CellColor = rawTeamData.Board.Cell_Color,
                    BorderColor = rawTeamData.Board.Border_Color,
                    BorderColors = rawTeamData.Board.Borders_Colors?.Split(';')
                }
            };
            return Ok(teamData);
        }

        [HttpPut("budget")]
        public async Task<IActionResult> UpdateBudget([FromBody] UpdateBudgetDto dto)
        {
            var team = await _context.Teams.FindAsync(dto.TeamId);
            if (team == null)
                return NotFound(CreateError("TEAM_NOT_FOUND", "teamNotFound"));

            team.Teams_Bud = dto.NewBudget;
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group($"game-{dto.GameId}-team-{dto.TeamId}").SendAsync("BudgetUpdated", dto.NewBudget);
            return Ok(new { message = "Budżet zaktualizowany." });
        }

        [HttpGet("currency")]
        public async Task<IActionResult> GetCurrency([FromQuery] int teamId)
        {
            if (teamId <= 0)
                return BadRequest(CreateError("INVALID_ID", "invalidId"));

            var teamBudget = await _context.Teams
                .Where(t => t.Teams_Id == teamId)
                .Select(t => (double?)t.Teams_Bud)
                .FirstOrDefaultAsync();

            if (teamBudget == null)
                return NotFound(CreateError("TEAM_NOT_FOUND", "teamNotFound"));

            return Ok(new { budget = teamBudget.Value });
        }

        [HttpGet("token-validate")]
        public async Task<IActionResult> ValidateToken([FromQuery] string token)
        {
            var tokenExists = await _context.Teams.AnyAsync(t => t.Teams_Token == token);
            if (!tokenExists)
                return NotFound(CreateError("INVALID_TOKEN", "invalidToken"));

            return Ok(new { message = "Token jest prawidłowy." });
        }

        [HttpGet("session-data")]
        public async Task<IActionResult> GetSessionByToken([FromQuery] string token)
        {
            try
            {
                var result = await _queryService.GetPlayerSessionDataAsync(token);
                if (result is ErrorResponseDto error)
                {
                    return Conflict(error);
                }
                return Ok(result);
            }
            catch
            {
                return StatusCode(500, CreateError("INTERNAL_ERROR", "internalError"));
            }
        }

        [HttpGet("unified-cards")]
        public async Task<IActionResult> GetUnifiedCards([FromQuery] int deckId, [FromQuery] int gameId, [FromQuery] int teamId)
        {
            try
            {
                var cards = await _queryService.GetCategorizedCardsForDeckAsync(deckId, gameId, teamId);
                return Ok(cards);
            }
            catch
            {
                return NotFound(CreateError("CARDS_NOT_FOUND", "cardsNotFound"));
            }
        }
    }
}