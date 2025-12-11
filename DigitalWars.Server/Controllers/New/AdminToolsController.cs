using backend.Data;
using DigitalWars.Server.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using backend.Hubs;
using Microsoft.AspNetCore.Authorization;

namespace DigitalWars.Server.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/[controller]")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminToolsController : BaseApiController
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<GameHub> _hubContext;

        public AdminToolsController(AppDbContext context, IHubContext<GameHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost("unlock-card")]
        public async Task<IActionResult> UnlockCard([FromBody] UnlockCardDto dto)
        {
            if (CurrentUserId == null) return Unauthorized(); // Tylko Admin

            var gameDeckId = await _context.Games.Where(g => g.Games_Id == dto.GameId).Select(g => g.Decks_Id).FirstOrDefaultAsync();
            if (gameDeckId == 0) return NotFound(CreateError("GAME_NOT_FOUND", "gameNotFound"));

            var card = await _context.Cards.FirstOrDefaultAsync(c => c.Card_Id == dto.CardId && c.Decks_Id == gameDeckId);
            if (card == null) return NotFound(CreateError("CARD_NOT_FOUND", "cardNotFound"));

            var enabler = new CardEnabler { Games_Id = dto.GameId, Teams_Id = dto.TeamId, Cards_Id = card.Cards_Id };
            _context.CardEnablers.Add(enabler);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Odblokowano." });
        }

        [HttpPost("apply-event")]
        public async Task<IActionResult> ApplyEvent([FromQuery] int gameId, [FromBody] ApplyEventDto dto)
        {
            if (CurrentUserId == null) return Unauthorized();
            var ev = await _context.GameEvents.FindAsync(dto.EventId);
            if (ev == null) return NotFound(CreateError("EVENT_NOT_FOUND", "eventNotFound"));
            var teams = await _context.Teams.Where(t => t.Games_Id == gameId).ToListAsync();
            foreach (var t in teams) { t.Games_Events_Id = dto.EventId; t.Turns_Left = ev.Turns_Time; }
            await _context.SaveChangesAsync();
            return Ok(new { message = "Zdarzenie aktywowane." });
        }

        [HttpPut("team-budget")]
        public async Task<IActionResult> UpdateBudget([FromBody] UpdateBudgetDto dto)
        {
            if (CurrentUserId == null) return Unauthorized();
            var team = await _context.Teams.FindAsync(dto.TeamId);
            if (team == null) return NotFound(CreateError("TEAM_NOT_FOUND", "teamNotFound"));
            team.Teams_Bud = dto.NewBudget;
            await _context.SaveChangesAsync();
            await _hubContext.Clients.Group($"game-{dto.GameId}-team-{dto.TeamId}").SendAsync("BudgetUpdated", dto.NewBudget);
            return Ok(new { message = "Budżet zaktualizowany." });
        }

        [HttpGet("licenses")]
        public async Task<IActionResult> GetLicenses()
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();
            var user = await _context.Users.FindAsync(userId.Value);
            return Ok(new { owned = user!.Licenses_Owned, used = user.Licenses_Used, inProgress = user.Games_In_Progress });
        }
    }
}