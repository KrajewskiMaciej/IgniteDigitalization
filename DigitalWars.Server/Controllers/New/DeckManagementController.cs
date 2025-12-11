using backend.Controllers;
using backend.Data;
using backend.Dtos;
using backend.Services;
using DigitalWars.Server.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DigitalWars.Server.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/[controller]")]
    public class DeckManagementController : BaseApiController
    {
        private readonly AppDbContext _context;
        private readonly IProvisioningService _provisioningService;

        public DeckManagementController(AppDbContext context, IProvisioningService provisioningService)
        {
            _context = context;
            _provisioningService = provisioningService;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetAllDecks()
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();

            var decks = await _context.Decks
                .AsNoTracking()
                .Where(deck => deck.Users_Id == userId.Value)
                .Select(deck => new { id = deck.Decks_Id, title = deck.Deck_Name })
                .ToListAsync();
            return Ok(decks);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateDeck(IFormFile file)
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();

            try
            {
                var newDeck = await _provisioningService.CreateDeckFromFileForUserAsync(file, userId.Value, file.FileName);
                return Ok(new { deckId = newDeck.Decks_Id });
            }
            catch (ArgumentException)
            {
                return BadRequest(CreateError("INVALID_FILE", "invalidFile"));
            }
            catch (Exception)
            {
                return StatusCode(500, CreateError("INTERNAL_ERROR", "internalError"));
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> EditDeckName([FromBody] EditDeckDto dto)
        {
            var userId = CurrentUserId;
            var deck = await _context.Decks.FirstOrDefaultAsync(d => d.Decks_Id == dto.Decks_Id && d.Users_Id == userId);

            if (deck == null) return NotFound(CreateError("DECK_NOT_FOUND", "deckNotFound"));
            if (string.IsNullOrWhiteSpace(dto.Decks_Name)) return BadRequest(CreateError("INVALID_NAME", "invalidName"));

            deck.Deck_Name = dto.Decks_Name;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Zaktualizowano nazwę." });
        }

        [HttpGet("template")]
        public IActionResult GetTemplate()
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "Templates", "DigitalWars_SzablonKart.xlsx");
            if (!System.IO.File.Exists(filePath))
                return NotFound(CreateError("FILE_NOT_FOUND", "fileNotFound"));

            return PhysicalFile(filePath, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DigitalWars_SzablonKart.xlsx");
        }
    }
}