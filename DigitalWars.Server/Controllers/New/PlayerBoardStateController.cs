using backend.Services;
using Microsoft.AspNetCore.Mvc;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;

namespace DigitalWars.Server.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/[controller]")]
    public class PlayerBoardStateController : BaseApiController
    {
        private readonly IPlayerQueryService _queryService;
        private readonly AppDbContext _context;

        public PlayerBoardStateController(IPlayerQueryService queryService, AppDbContext context)
        {
            _queryService = queryService;
            _context = context;
        }

        [HttpGet("team-pawns")]
        public async Task<IActionResult> GetTeamPawns([FromQuery] int boardId, [FromQuery] int? teamId = null)
        {
            int effectiveTeamId;
            try { effectiveTeamId = ResolveTeamId(teamId); }
            catch (ArgumentException ex) { return BadRequest(CreateError("MISSING_PARAM", ex.Message)); }

            if (teamId == null) return Unauthorized();
            var gameId = await _context.Teams.Where(t => t.Teams_Id == teamId).Select(t => t.Games_Id).FirstOrDefaultAsync();

            var pawns = await _queryService.GetProcessPawnsForBoardAsync(gameId, teamId.Value, boardId);
            return Ok(pawns);
        }

        [HttpGet("rival-pawns")]
        public async Task<IActionResult> GetRivalPawns([FromQuery] int boardId, [FromQuery] int? teamId = null)
        {
            int effectiveTeamId;
            try { effectiveTeamId = ResolveTeamId(teamId); }
            catch (ArgumentException ex) { return BadRequest(CreateError("MISSING_PARAM", ex.Message)); }

            if (teamId == null) return Unauthorized();
            var gameId = await _context.Teams.Where(t => t.Teams_Id == teamId).Select(t => t.Games_Id).FirstOrDefaultAsync();

            var pawns = await _context.GameBoards
               .Include(p => p.Teams)
               .Where(p => p.Games_Id == gameId && p.Boards_Id == boardId && p.Games_Processes_Id == null)
               .Select(p => new { TeamId = p.Teams_Id, PosX = p.Poz_X, PosY = p.Poz_Y, TeamColor = p.Teams.Teams_Color, TeamName = p.Teams.Teams_Name })
               .ToListAsync();
            return Ok(pawns);
        }

        [HttpGet("rival-config")]
        public async Task<IActionResult> GetRivalBoardConfig([FromQuery] int? teamId = null)
        {
            int effectiveTeamId;
            try { effectiveTeamId = ResolveTeamId(teamId); }
            catch (ArgumentException ex) { return BadRequest(CreateError("MISSING_PARAM", ex.Message)); }

            if (teamId == null) return Unauthorized();
            var gameId = await _context.Teams.Where(t => t.Teams_Id == teamId).Select(t => t.Games_Id).FirstOrDefaultAsync();

            var game = await _context.Games.AsNoTracking().Include(g => g.Rivals_Boards).FirstOrDefaultAsync(g => g.Games_Id == gameId);
            if (game?.Rivals_Boards == null) return NotFound(CreateError("CONFIG_NOT_FOUND", "configNotFound"));
            var c = game.Rivals_Boards;
            return Ok(new { rivalBoardConfig = new { boardId = c.Boards_Id, name = c.Name, labelsUp = c.Labels_Up?.Split(';'), labelsRight = c.Labels_Right?.Split(';'), rows = c.Rows, cols = c.Cols, cellColor = c.Cell_Color, borderColor = c.Border_Color, borderColors = c.Borders_Colors?.Split(';') } });
        }
    }
}