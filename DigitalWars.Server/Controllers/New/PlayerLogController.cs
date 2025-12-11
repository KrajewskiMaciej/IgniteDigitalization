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

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingLogs([FromQuery] int gameId, [FromQuery] int? teamId)
        {
            var query = _context.GameLogs
                .Where(l => l.Games_Id == gameId && l.Is_Approved == false && l.Teams != null && l.Cards != null);

            if (teamId.HasValue)
                query = query.Where(l => l.Teams_Id == teamId.Value);

            var logs = await query.Select(l => new
            {
                LogId = l.Games_Logs_Id,
                TeamName = l.Teams!.Teams_Name,
                CardId = l.Cards!.Card_Id,
                InternalCardId = l.Cards_Id,
                l.Costs,
                Timestamp = l.Data
            }).ToListAsync();

            var validLogs = logs.Where(l => l.InternalCardId.HasValue).ToList();
            var cardIds = validLogs.Select(l => l.InternalCardId!.Value).ToList();

            var decisionTitles = await _context.Decisions.Where(d => cardIds.Contains(d.Cards_Id)).ToDictionaryAsync(d => d.Cards_Id, d => d.Decisions_Short_Desc);
            var hardwareTitles = await _context.Hardwares.Where(h => cardIds.Contains(h.Cards_Id)).ToDictionaryAsync(h => h.Cards_Id, h => h.Hardwares_Short_Desc);
            var softwareTitles = await _context.Softwares.Where(s => cardIds.Contains(s.Cards_Id)).ToDictionaryAsync(s => s.Cards_Id, s => s.Softwares_Short_Desc);

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

        [HttpPost("approve")]
        public async Task<IActionResult> ApproveLog([FromQuery] int logId)
        {
            try
            {
                await _actionService.ApproveLogAsync(logId);
                return Ok(new { message = "Zatwierdzono." });
            }
            catch
            {
                return BadRequest(CreateError("APPROVAL_ERROR", "approvalError"));
            }
        }

        [HttpDelete("reject")]
        public async Task<IActionResult> RejectLog([FromQuery] int logId)
        {
            try
            {
                await _actionService.RejectLogAsync(logId);
                return Ok(new { message = "Odrzucono." });
            }
            catch
            {
                return BadRequest(CreateError("REJECTION_ERROR", "rejectionError"));
            }
        }

        [HttpPost("history")]
        public async Task<IActionResult> GetHistory([FromBody] PlayerHistoryRequestDto request)
        {
            var query = _context.GameLogs
                .Include(l => l.Teams)
                .Include(l => l.Cards)
                .Where(l => l.Games_Id == request.GameId && l.Is_Approved == true);

            if (request.TeamId.HasValue)
                query = query.Where(l => l.Teams_Id == request.TeamId.Value);

            var rawLogs = await query.OrderByDescending(l => l.Data).ToListAsync();

            return Ok(rawLogs.Select(l => new
            {
                l.Data,
                l.Teams?.Teams_Name,
                l.Cards?.Card_Id,
                Status = l.Status
            }));
        }
    }
}