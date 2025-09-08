using backend.Data;
using backend.DTOs; // Upewnij się, że ten plik istnieje i zawiera UpdateItemDto
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
    // DTO do aktualizacji przedmiotu
    public class UpdateItemDto
    {
        public string ShortDesc { get; set; } = string.Empty;
        public string LongDesc { get; set; } = string.Empty;
    }

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
                .Select(d => new { id = d.Card.Card_Id, deckId = d.Card.Decks_Id, title = d.Decisions_Short_Desc, description = d.Decisions_Long_Desc })
                .ToListAsync();

            return Ok(decisionCards);
        }

        [HttpGet("items")]
        public async Task<IActionResult> GetItemsForDeck([FromQuery] int deckId)
        {
            var hardwareItems = await _context.Hardwares
                .Include(h => h.Cards)
                .Where(h => h.Cards.Decks_Id == deckId)
                .Select(h => new
                {
                    id = h.Cards.Card_Id,
                    deckId = h.Cards.Decks_Id,
                    shortDesc = h.Hardwares_Short_Desc,
                    longDesc = h.Hardwares_Long_Desc,
                    type = "Hardware"
                }).ToListAsync();

            var softwareItems = await _context.Softwares
                .Include(s => s.Cards)
                .Where(s => s.Cards.Decks_Id == deckId)
                .Select(s => new
                {
                    id = s.Cards.Card_Id,
                    deckId = s.Cards.Decks_Id,
                    shortDesc = s.Softwares_Short_Desc,
                    longDesc = s.Softwares_Long_Desc,
                    type = "Software"
                }).ToListAsync();

            var allItems = hardwareItems
                .AsEnumerable()
                .Concat(softwareItems)
                .OrderBy(i => i.id);

            return Ok(allItems);
        }

        [HttpPut("items/{cardId}")]
        public async Task<IActionResult> UpdateItem(int cardId, [FromBody] UpdateItemDto dto)
        {
            var hardware = await _context.Hardwares.FirstOrDefaultAsync(h => h.Cards.Card_Id == cardId);
            if (hardware != null)
            {
                hardware.Hardwares_Short_Desc = dto.ShortDesc;
                hardware.Hardwares_Long_Desc = dto.LongDesc;
            }
            else
            {
                var software = await _context.Softwares.FirstOrDefaultAsync(s => s.Cards.Card_Id == cardId);
                if (software != null)
                {
                    software.Softwares_Short_Desc = dto.ShortDesc;
                    software.Softwares_Long_Desc = dto.LongDesc;
                }
                else
                {
                    return NotFound("Przedmiot o podanym ID nie został znaleziony.");
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Przedmiot został pomyślnie zaktualizowany." });
        }
    }
}

