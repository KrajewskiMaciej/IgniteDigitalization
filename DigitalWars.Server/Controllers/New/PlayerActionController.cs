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
    public class PlayerActionController : BaseApiController
    {
        private readonly IPlayerActionService _actionService;
        private readonly AppDbContext _context;
        private readonly ILogger<PlayerActionController> _logger;

        public PlayerActionController(IPlayerActionService actionService, AppDbContext context, ILogger<PlayerActionController> logger)
        {
            _actionService = actionService;
            _context = context;
            _logger = logger;
        }

        [HttpPost("success")]
        public async Task<IActionResult> PlayCardSuccess([FromBody] CardDataDto cardData)
        {
            return await HandleCardPlay(cardData.CardId, cardData, true);
        }

        [HttpPost("failure")]
        public async Task<IActionResult> PlayCardFailure([FromBody] CardDataDto cardData)
        {
            return await HandleCardPlay(cardData.CardId, cardData, false);
        }

        private async Task<IActionResult> HandleCardPlay(int cardId, CardDataDto cardData, bool success)
        {
            try
            {
                var result = await _actionService.PlayCardAsync(cardId, cardData, wasSuccess: success);
                return Ok(result);
            }
            catch (GameException ex)
            {
                return Conflict(CreateError("GAME_EXCEPTION", ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas zagrywania karty");
                return StatusCode(500, CreateError("INTERNAL_ERROR", "internalError"));
            }
        }

        [HttpPost("unlock-card")]
        public async Task<IActionResult> UnlockCard([FromBody] UnlockCardDto dto)
        {
            var gameDeckId = await _context.Games
                .Where(g => g.Games_Id == dto.GameId)
                .Select(g => g.Decks_Id)
                .FirstOrDefaultAsync();

            if (gameDeckId == 0)
                return NotFound(CreateError("GAME_NOT_FOUND", "gameNotFound"));

            var card = await _context.Cards.FirstOrDefaultAsync(c => c.Card_Id == dto.CardId && c.Decks_Id == gameDeckId);
            if (card == null)
                return NotFound(CreateError("CARD_NOT_FOUND", "cardNotFound"));

            var enabler = new CardEnabler
            {
                Games_Id = dto.GameId,
                Teams_Id = dto.TeamId,
                Cards_Id = card.Cards_Id
            };
            _context.CardEnablers.Add(enabler);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Karta została odblokowana." });
        }
    }
}