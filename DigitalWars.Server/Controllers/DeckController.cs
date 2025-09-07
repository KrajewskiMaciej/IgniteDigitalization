using backend.Data;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/admin/deck")]
    public class DeckController : BaseApiController
    {
        private readonly AppDbContext _context;
        private readonly IProvisioningService _provisioningService;

        public DeckController(AppDbContext context, IProvisioningService provisioningService)
        {
            _context = context;
            _provisioningService = provisioningService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            var userId = CurrentUserId;
            var userName = User.FindFirstValue(ClaimTypes.Name);
            if (userId == null || string.IsNullOrEmpty(userName))
                return Unauthorized("Brak danych użytkownika.");

            try
            {
                var newDeck = await _provisioningService.CreateDeckFromFileForUserAsync(file, userId.Value, file.FileName);
                return Ok(new { message = "Dane zapisane pomyślnie", deckId = newDeck.Decks_Id });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Wystąpił krytyczny błąd serwera: {ex.Message}");
            }
        }

        [HttpGet("get")]
        public async Task<IActionResult> GetDecks()
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized("Brak danych użytkownika.");

            var decks = await _context.Decks
                .AsNoTracking()
                .Where(deck => deck.Users_Id == userId.Value)
                .Select(deck => new { id = deck.Decks_Id, title = deck.Deck_Name })
                .ToListAsync();

            return Ok(decks);
        }

        [HttpGet("decisions")]
        public async Task<IActionResult> GetDecisionCards(int deckId)
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized("Brak danych użytkownika.");

            var deckExists = await _context.Decks
                .AsNoTracking()
                .AnyAsync(d => d.Decks_Id == deckId && (d.Users_Id == null || d.Users_Id == userId.Value));

            if (!deckExists) return NotFound("Talia nie istnieje lub brak dostępu.");

            var decisionCards = await _context.Decisions
                .AsNoTracking()
                .Include(d => d.Card)
                .Where(d => d.Card.Decks_Id == deckId)
                .Select(d => new { id = d.Cards_Id, deckId = d.Card.Decks_Id, title = d.Decisions_Short_Desc, description = d.Decisions_Long_Desc })
                .ToListAsync();

            return Ok(decisionCards);
        }
    }
}