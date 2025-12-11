using DigitalWars.Server.Dtos;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace DigitalWars.Server.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/[controller]")]
    public class PlayerResourcesController : BaseApiController
    {
        private readonly IPlayerQueryService _queryService;
        private readonly AppDbContext _context;

        public PlayerResourcesController(IPlayerQueryService queryService, AppDbContext context)
        {
            _queryService = queryService;
            _context = context;
        }

        [HttpGet("unified-cards")]
        public async Task<IActionResult> GetUnifiedCards([FromQuery] int? teamId = null)
        {
            int effectiveTeamId;
            try { effectiveTeamId = ResolveTeamId(teamId); }
            catch (ArgumentException ex) { return BadRequest(CreateError("MISSING_PARAM", ex.Message)); }

            if (teamId == null) return Unauthorized();

            var teamInfo = await _context.Teams.Include(t => t.Games)
                .Where(t => t.Teams_Id == teamId).Select(t => new { t.Games_Id, t.Games.Decks_Id }).FirstOrDefaultAsync();

            if (teamInfo == null) return NotFound(CreateError("GAME_DATA_NOT_FOUND", "gameDataError"));

            try { return Ok(await _queryService.GetCategorizedCardsForDeckAsync(teamInfo.Decks_Id, teamInfo.Games_Id, teamId.Value)); }
            catch { return NotFound(CreateError("CARDS_NOT_FOUND", "cardsNotFound")); }
        }

        [HttpGet("game-events")]
        public async Task<IActionResult> GetGameEvents([FromQuery] int? teamId = null)
        {
            int effectiveTeamId;
            try { effectiveTeamId = ResolveTeamId(teamId); }
            catch (ArgumentException ex) { return BadRequest(CreateError("MISSING_PARAM", ex.Message)); }

            if (teamId == null) return Unauthorized();

            var deckId = await _context.Teams.Where(t => t.Teams_Id == teamId).Select(t => t.Games.Decks_Id).FirstOrDefaultAsync();

            return Ok(await _context.GameEvents.Where(e => e.Decks_Id == deckId)
                .Select(e => new GameEventDto { EventId = e.Games_Events_Id, ShortDesc = e.Events_Short_Desc, LongDesc = e.Events_Long_Desc }).ToListAsync());
        }
    }
}