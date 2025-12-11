using backend.Services;
using DigitalWars.Server.Dtos;
using Microsoft.AspNetCore.Mvc;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace DigitalWars.Server.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/[controller]")]
    public class PlayerLogController : BaseApiController
    {
        private readonly AppDbContext _context;
        private readonly IPlayerActionService _actionService;

        public PlayerLogController(AppDbContext context, IPlayerActionService actionService)
        {
            _context = context;
            _actionService = actionService;
        }

        [HttpGet("read")]
        public async Task<IActionResult> GetHistory([FromQuery] int? teamId = null)
        {
            int effectiveTeamId;
            try
            {
                effectiveTeamId = ResolveTeamId(teamId);
            }
            catch (ArgumentException ex) { return BadRequest(CreateError("MISSING_PARAM", ex.Message)); }

            if (teamId == null) return Unauthorized();
            var gameId = await _context.Teams.Where(t => t.Teams_Id == teamId).Select(t => t.Games_Id).FirstOrDefaultAsync();

            var query = _context.GameLogs.Include(l => l.Teams).Include(l => l.Cards)
                .Where(l => l.Games_Id == gameId && l.Teams_Id == teamId.Value && l.Is_Approved == true);

            var rawLogs = await query.OrderByDescending(l => l.Data).ToListAsync();
            return Ok(rawLogs.Select(l => new { l.Data, l.Teams?.Teams_Name, l.Cards?.Card_Id, Status = l.Status }));
        }

        [HttpGet("decision-waiting")]
        public async Task<IActionResult> GetPendingLogs([FromQuery] int? teamId = null)
        {
            int effectiveTeamId;
            try { effectiveTeamId = ResolveTeamId(teamId); }
            catch (ArgumentException ex) { return BadRequest(CreateError("MISSING_PARAM", ex.Message)); }

            if (teamId == null) return Unauthorized();
            var gameId = await _context.Teams.Where(t => t.Teams_Id == teamId).Select(t => t.Games_Id).FirstOrDefaultAsync();

            var query = _context.GameLogs.Where(l => l.Games_Id == gameId && l.Teams_Id == teamId && l.Is_Approved == false);
            return Ok(await query.ToListAsync());
        }

        [HttpPost("decision-approve")]
        public async Task<IActionResult> ApproveLog([FromQuery] int logId)
        {
            try { await _actionService.ApproveLogAsync(logId); return Ok(new { message = "Zatwierdzono." }); }
            catch { return BadRequest(CreateError("APPROVAL_ERROR", "approvalError")); }
        }

        [HttpDelete("decision-reject")]
        public async Task<IActionResult> RejectLog([FromQuery] int logId)
        {
            try { await _actionService.RejectLogAsync(logId); return Ok(new { message = "Odrzucono." }); }
            catch { return BadRequest(CreateError("REJECTION_ERROR", "rejectionError")); }
        }
    }
}