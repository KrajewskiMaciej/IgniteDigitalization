using DigitalWars.Server.Dtos;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace DigitalWars.Server.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/[controller]")]
    public class PlayerSessionController : BaseApiController
    {
        private readonly IPlayerQueryService _queryService;
        private readonly AppDbContext _context;

        public PlayerSessionController(IPlayerQueryService queryService, AppDbContext context)
        {
            _queryService = queryService;
            _context = context;
        }

        [HttpGet("info")]
        public async Task<IActionResult> GetTeamInfo([FromQuery] int? teamId = null)
        {
            int effectiveTeamId;
            try
            {
                effectiveTeamId = ResolveTeamId(teamId);
            }
            catch (ArgumentException ex) { return BadRequest(CreateError("MISSING_PARAM", ex.Message)); }

            var team = await _context.Teams.Include(t => t.Games).ThenInclude(g => g.Teams_Boards)
                .FirstOrDefaultAsync(t => t.Teams_Id == effectiveTeamId);

            if (team == null) return NotFound(CreateError("TEAM_NOT_FOUND", "teamNotFound"));
            var teamData = new SessionDataDto
            {
                TeamId = team.Teams_Id,
                TeamName = team.Teams_Name,
                TeamColor = team.Teams_Color,
                TeamBud = team.Teams_Bud,
                DeckId = team.Games.Decks_Id,
                BoardConfig = new BoardConfigDto { BoardId = team.Games.Teams_Boards.Boards_Id, Name = team.Games.Teams_Boards.Name, LabelsUp = team.Games.Teams_Boards.Labels_Up?.Split(';')!, LabelsRight = team.Games.Teams_Boards.Labels_Right?.Split(';')!, Rows = team.Games.Teams_Boards.Rows, Cols = team.Games.Teams_Boards.Cols, CellColor = team.Games.Teams_Boards.Cell_Color, BorderColor = team.Games.Teams_Boards.Border_Color, BorderColors = team.Games.Teams_Boards.Borders_Colors?.Split(';')! }
            };
            return Ok(teamData);
        }

        [HttpGet("currency")]
        public async Task<IActionResult> GetCurrency([FromQuery] int? teamId = null)
        {
            int effectiveTeamId;
            try { effectiveTeamId = ResolveTeamId(teamId); }
            catch (ArgumentException ex) { return BadRequest(CreateError("MISSING_PARAM", ex.Message)); }

            var teamBudget = await _context.Teams.Where(t => t.Teams_Id == effectiveTeamId).Select(t => (double?)t.Teams_Bud).FirstOrDefaultAsync();
            if (teamBudget == null) return NotFound(CreateError("TEAM_NOT_FOUND", "teamNotFound"));
            return Ok(new { budget = teamBudget.Value });
        }

        [HttpGet("full-state")]
        public async Task<IActionResult> GetFullSessionState([FromQuery] int? teamId = null)
        {
            int effectiveTeamId;
            try { effectiveTeamId = ResolveTeamId(teamId); }
            catch (ArgumentException ex) { return BadRequest(CreateError("MISSING_PARAM", ex.Message)); }

            if (teamId == null) return Unauthorized();

            var token = await _context.Teams.Where(t => t.Teams_Id == teamId).Select(t => t.Teams_Token).FirstOrDefaultAsync();

            try
            {
                var result = await _queryService.GetPlayerSessionDataAsync(token!);
                if (result is ErrorResponseDto error) return Conflict(error);
                return Ok(result);
            }
            catch { return StatusCode(500, CreateError("INTERNAL_ERROR", "internalError")); }
        }
    }
}