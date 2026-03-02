using backend.Data;
using backend.PdfGeneration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using backend.Dtos;

namespace backend.Controllers
{
    public class BoardExportRequest { public int TeamBoardId { get; set; } public int RivalBoardId { get; set; } }

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : BaseApiController
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
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
                .Include(d => d.Card).ThenInclude(c => c.Phase)
                .Where(d => d.Card.Decks_Id == deckId)
                .Select(d => new CardPdfModel
                {
                    Id = d.Cards_Id,
                    Title = d.Decisions_Short_Desc,
                    Description = d.Decisions_Long_Desc,
                    CardType = "Decision",
                    Cost = d.Decisions_Cost_Bits,
                    Phase = d.Card.Phase != null ? d.Card.Phase.Phase_Name : string.Empty
                })
                .ToListAsync();

            var hardwareCards = await _context.Hardwares
                .Include(d => d.Cards).ThenInclude(c => c.Phase)
                .Where(i => i.Cards.Decks_Id == deckId)
                .Select(i => new CardPdfModel
                {
                    Id = i.Cards_Id,
                    Title = i.Hardwares_Short_Desc,
                    Description = i.Hardwares_Long_Desc,
                    CardType = "Hardware",
                    Cost = i.Hardwares_Cost_Bits,
                    Phase = i.Cards.Phase != null ? i.Cards.Phase.Phase_Name : string.Empty
                })
                .ToListAsync();

            var softwareCards = await _context.Softwares
                .Include(d => d.Cards).ThenInclude(c => c.Phase)
                .Where(i => i.Cards.Decks_Id == deckId)
                .Select(i => new CardPdfModel
                {
                    Id = i.Cards_Id,
                    Title = i.Softwares_Short_Desc,
                    Description = i.Softwares_Long_Desc,
                    CardType = "Software",
                    Cost = i.Softwares_Cost_Bits,
                    Phase = i.Cards.Phase != null ? i.Cards.Phase.Phase_Name : string.Empty
                })
                .ToListAsync();

            var allCards = decisionCards.Concat(hardwareCards).Concat(softwareCards).OrderBy(c => c.Id).ToList();
            if (!allCards.Any()) return NotFound($"Brak kart dla talii o ID {deckId}.");

            var document = new CardsDocument(allCards);
            byte[] pdfBytes = document.GeneratePdf();

            return File(pdfBytes, "application/pdf", "DigitalWars_Karty.pdf");
        }

        [HttpGet("exportBoards")]
        // Zmieniono parametry z [FromBody] na [FromQuery]
        public async Task<IActionResult> GenerateBoardsPdf([FromQuery] int teamBoardId, [FromQuery] int rivalBoardId)
        {
            var boards = await _context.Boards
                .AsNoTracking()
                // Użyto nowych parametrów
                .Where(b => b.Boards_Id == teamBoardId || b.Boards_Id == rivalBoardId)
                .ToListAsync();

            if (boards.Count < 2) return NotFound("Nie znaleziono jednej lub obu plansz.");

            // Użyto nowych parametrów
            var document = new BoardsDocument(boards, teamBoardId, rivalBoardId);
            byte[] pdfBytes = document.GeneratePdf();

            return File(pdfBytes, "application/pdf", "DigitalWars_Plansze.pdf");
        }

        [Authorize]
        [HttpGet("game/{gameId}/teams-management")]
        public async Task<IActionResult> GetTeamsManagement(int gameId)
        {
            var teams = await _context.Teams
                .Where(t => t.Games_Id == gameId)
                .Select(t => new TeamManagementDto
                {
                    TeamId = t.Teams_Id,
                    TeamName = t.Teams_Name,
                    TeamBud = t.Teams_Bud,
                    TeamToken = t.Teams_Token,
                    TeamColor = t.Teams_Color,
                    DeckId = t.Games.Decks_Id,
                    BoardId = t.Games.Teams_Boards_Id
                }).ToListAsync();
            return Ok(teams);
        }
    }
}