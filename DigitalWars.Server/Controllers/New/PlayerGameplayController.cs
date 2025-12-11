using DigitalWars.Server.Dtos;
using DigitalWars.Server.Services;
using Microsoft.AspNetCore.Mvc;
using backend.Exceptions;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace DigitalWars.Server.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/[controller]")]
    public class PlayerGameplayController : BaseApiController
    {
        private readonly IPlayerActionService _actionService;
        private readonly AppDbContext _context;
        private readonly ILogger<PlayerGameplayController> _logger;

        public PlayerGameplayController(IPlayerActionService actionService, AppDbContext context, ILogger<PlayerGameplayController> logger)
        {
            _actionService = actionService;
            _context = context;
            _logger = logger;
        }

        [HttpPost("success")]
        public async Task<IActionResult> PlayCardSuccess([FromBody] CardDataDto cardData)
        {
            int effectiveTeamId;
            try { effectiveTeamId = ResolveTeamId(cardData.TeamId); }
            catch (ArgumentException ex) { return BadRequest(CreateError("MISSING_PARAM", ex.Message)); }

            return await HandleCardPlay(cardData.CardId, cardData, true);
        }

        [HttpPost("failure")]
        public async Task<IActionResult> PlayCardFailure([FromBody] CardDataDto cardData)
        {

            int effectiveTeamId;
            try { effectiveTeamId = ResolveTeamId(cardData.TeamId); }
            catch (ArgumentException ex) { return BadRequest(CreateError("MISSING_PARAM", ex.Message)); }

            return await HandleCardPlay(cardData.CardId, cardData, false);
        }

        private async Task<IActionResult> HandleCardPlay(int cardId, CardDataDto cardData, bool success)
        {
            try
            {
                var result = await _actionService.PlayCardAsync(cardId, cardData, wasSuccess: success);
                return Ok(result);
            }
            catch (GameException ex) { return Conflict(CreateError("GAME_EXCEPTION", ex.Message)); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas zagrywania karty");
                return StatusCode(500, CreateError("INTERNAL_ERROR", "internalError"));
            }
        }
    }
}