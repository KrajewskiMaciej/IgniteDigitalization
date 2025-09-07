using backend.Data;
using backend.PdfGeneration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Controllers
{
    public class BoardExportRequest { public int TeamBoardId { get; set; } public int RivalBoardId { get; set; } }

    [Authorize]
    [ApiController]
    [Route("api/admin")]
    public class UserController : BaseApiController
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        [HttpGet("licenses")]
        public async Task<IActionResult> GetUserLicenses()
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null) return NotFound("Użytkownik nie istnieje.");

            return Ok(new
            {
                licensesOwned = user.Licenses_Owned,
                licensesUsed = user.Licenses_Used,
                licensesLeft = user.Licenses_Owned - user.Licenses_Used
            });
        }

        [HttpGet("exportCards")]
        public async Task<IActionResult> GenerateCardsPdf([FromQuery] int deckId)
        {
            var decisionCards = await _context.Decisions
                .Include(d => d.Card)
                .Where(d => d.Card.Decks_Id == deckId)
                .Select(d => new CardPdfModel { Id = d.Cards_Id, Title = d.Decisions_Short_Desc, Description = d.Decisions_Long_Desc, CardType = "Decision" })
                .ToListAsync();

            var hardwareCards = await _context.Hardwares
                 .Include(d => d.Cards)
                .Where(i => i.Cards.Decks_Id == deckId)
                .Select(i => new CardPdfModel { Id = i.Cards_Id, Title = i.Hardwares_Short_Desc, Description = i.Hardwares_Long_Desc, CardType = "Hardware" })
                .ToListAsync();

            var softwareCards = await _context.Softwares
                 .Include(d => d.Cards)
                .Where(i => i.Cards.Decks_Id == deckId)
                .Select(i => new CardPdfModel { Id = i.Cards_Id, Title = i.Softwares_Short_Desc, Description = i.Softwares_Long_Desc, CardType = "Software" })
                .ToListAsync();

            var allCards = decisionCards.Concat(hardwareCards).Concat(softwareCards).OrderBy(c => c.Id).ToList();
            if (!allCards.Any()) return NotFound($"Brak kart dla talii o ID {deckId}.");

            var document = new CardsDocument(allCards);
            byte[] pdfBytes = document.GeneratePdf();

            return File(pdfBytes, "application/pdf", "DigitalWars_Karty.pdf");
        }

        [HttpPost("exportBoards")]
        public async Task<IActionResult> GenerateBoardsPdf([FromBody] BoardExportRequest request)
        {
            var boards = await _context.Boards
                .AsNoTracking()
                .Where(b => b.Boards_Id == request.TeamBoardId || b.Boards_Id == request.RivalBoardId)
                .ToListAsync();

            if (boards.Count < 2) return NotFound("Nie znaleziono jednej lub obu plansz.");

            var document = new BoardsDocument(boards, request.TeamBoardId, request.RivalBoardId);
            byte[] pdfBytes = document.GeneratePdf();

            return File(pdfBytes, "application/pdf", "DigitalWars_Plansze.pdf");
        }
    }
}