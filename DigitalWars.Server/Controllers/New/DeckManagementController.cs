using backend.Data;
using DigitalWars.Server.Dtos;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalWars.Server.PdfGeneration;
using QuestPDF.Fluent;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;

namespace DigitalWars.Server.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/[controller]")]
    [Authorize(Policy = "AdminOnly")]
    public class DeckManagementController : BaseApiController
    {
        private readonly AppDbContext _context;
        private readonly IProvisioningService _provisioningService;

        public DeckManagementController(AppDbContext context, IProvisioningService provisioningService)
        {
            _context = context;
            _provisioningService = provisioningService;
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
            catch (ArgumentException) { return BadRequest(CreateError("INVALID_FILE", "invalidFile")); }
            catch (Exception) { return StatusCode(500, CreateError("INTERNAL_ERROR", "internalError")); }
        }

        [HttpGet("read")]
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

        [HttpGet("export-pdf")]
        public async Task<IActionResult> GenerateCardsPdf([FromQuery] int deckId)
        {
            if (CurrentUserId == null) return Unauthorized();

            var decisionCards = await _context.Decisions.Include(d => d.Card).Where(d => d.Card.Decks_Id == deckId)
                .Select(d => new CardPdfModel { Id = d.Cards_Id, Title = d.Decisions_Short_Desc, Description = d.Decisions_Long_Desc, CardType = "Decision" }).ToListAsync();
            var hardwareCards = await _context.Hardwares.Include(d => d.Cards).Where(i => i.Cards.Decks_Id == deckId)
                .Select(i => new CardPdfModel { Id = i.Cards_Id, Title = i.Hardwares_Short_Desc, Description = i.Hardwares_Long_Desc, CardType = "Hardware" }).ToListAsync();
            var softwareCards = await _context.Softwares.Include(d => d.Cards).Where(i => i.Cards.Decks_Id == deckId)
                .Select(i => new CardPdfModel { Id = i.Cards_Id, Title = i.Softwares_Short_Desc, Description = i.Softwares_Long_Desc, CardType = "Software" }).ToListAsync();

            var allCards = decisionCards.Concat(hardwareCards).Concat(softwareCards).OrderBy(c => c.Id).ToList();
            if (!allCards.Any()) return NotFound(CreateError("NO_CARDS", "noCardsFound"));

            var document = new CardsDocument(allCards);
            return File(document.GeneratePdf(), "application/pdf", "DigitalWars_Karty.pdf");
        }
    }
}