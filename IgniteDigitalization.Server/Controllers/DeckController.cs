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
using backend.Dtos;
using Microsoft.CodeAnalysis.Differencing;

namespace backend.Controllers
{
    // DTO do aktualizacji przedmiotu
    [Authorize]
    [ApiController]
    [Route("api/admin/deck")]
    public class DeckController : BaseApiController
    {
        private readonly AppDbContext _context;
        private readonly IProvisioningService _provisioningService;
        private readonly IEconomyService _economyService;

        public DeckController(AppDbContext context, IProvisioningService provisioningService, IEconomyService economyService)
        {
            _context = context;
            _provisioningService = provisioningService;
            _economyService = economyService;
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

        [HttpGet("download-template")]
        public IActionResult DownloadTemplate()
        {
            try
            {
                var fileName = "IgniteDigitalization_SzablonKart.xlsx";
                var filePath = Path.Combine(AppContext.BaseDirectory, "Templates", fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new { message = "Plik szablonu nie został znaleziony na serwerze." });
                }

                // Typ MIME dla plików .xlsx
                var mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                // Zwraca plik, który przeglądarka potraktuje jako plik do pobrania
                return PhysicalFile(filePath, mimeType, fileName);
            }
            catch (Exception ex)
            {
                // Logowanie błędu byłoby tu wskazane
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Wystąpił wewnętrzny błąd serwera: {ex.Message}" });
            }
        }

        [HttpPut("edit")]
        public async Task<IActionResult> EditDeck([FromBody] EditDeckDto dto)
        {
            var userId = CurrentUserId;
            if (userId == null)
                return Unauthorized("Brak danych uwierzytelniającego użytkownika.");

            var deckToEdit = await _context.Decks
                .FirstOrDefaultAsync(deck => deck.Decks_Id == dto.Decks_Id && deck.Users_Id == userId.Value);

            if (deckToEdit == null)
                return NotFound("Nie znaleziono szkolenia o podanym ID lub nie masz do niego uprawnień.");

            if (string.IsNullOrWhiteSpace(dto.Decks_Name))
                return BadRequest("Nazwa Szkolenia nie może być pusta.");

            deckToEdit.Deck_Name = dto.Decks_Name;

            // Aktualizacja domyślnych plansz (jeśli podano)
            if (dto.Default_Teams_Boards_Id.HasValue)
            {
                var boardExists = await _context.Boards.AnyAsync(b => b.Boards_Id == dto.Default_Teams_Boards_Id.Value && b.Users_Id == userId.Value);
                if (!boardExists) return BadRequest("Domyślna plansza drużynowa nie istnieje lub nie należy do Ciebie.");
                deckToEdit.Default_Teams_Boards_Id = dto.Default_Teams_Boards_Id.Value;
            }
            else
            {
                deckToEdit.Default_Teams_Boards_Id = null;
            }

            if (dto.Default_Rivals_Boards_Id.HasValue)
            {
                var boardExists = await _context.Boards.AnyAsync(b => b.Boards_Id == dto.Default_Rivals_Boards_Id.Value && b.Users_Id == userId.Value);
                if (!boardExists) return BadRequest("Domyślna plansza rynkowa nie istnieje lub nie należy do Ciebie.");
                deckToEdit.Default_Rivals_Boards_Id = dto.Default_Rivals_Boards_Id.Value;
            }
            else
            {
                deckToEdit.Default_Rivals_Boards_Id = null;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Szkolenie zostało pomyślnie zaktualizowane." });
        }

        [HttpGet("get")]
        public async Task<IActionResult> GetDecks()
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized("Brak danych użytkownika.");

            var decks = await _context.Decks
                .AsNoTracking()
                .Where(deck => deck.Users_Id == userId.Value)
                .Select(deck => new DeckListItemDto
                {
                    Id = deck.Decks_Id,
                    Title = deck.Deck_Name,
                    DefaultTeamsBoardId = deck.Default_Teams_Boards_Id,
                    DefaultRivalsBoardId = deck.Default_Rivals_Boards_Id,
                    DefaultTeamsBoardName = deck.DefaultTeamsBoard != null ? deck.DefaultTeamsBoard.Name : null,
                    DefaultRivalsBoardName = deck.DefaultRivalsBoard != null ? deck.DefaultRivalsBoard.Name : null,
                })
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

        [HttpPut("decisions/edit")]
        public async Task<IActionResult> EditDecisionCards([FromBody] UpdateCardDto dto)
        {
            var decision = await _context.Decisions
                .Include(d => d.Card)
                .FirstOrDefaultAsync(d => d.Card.Card_Id == dto.CardId);

            if (decision == null)
            {
                return NotFound("Decyzja o podanym ID nie została znaleziona.");
            }

            decision.Decisions_Short_Desc = dto.ShortDesc;
            decision.Decisions_Long_Desc = dto.LongDesc;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Decyzja została pomyślnie zaktualizowana." });
        }

        [HttpGet("feedbacks")]
        public async Task<IActionResult> GetCardFeedbacks([FromQuery] int cardId)
        {
            var feedbacks = await _context.Feedbacks
                .AsNoTracking()
                .Include(f => f.Cards)
                .Where(f => f.Cards.Card_Id == cardId)
                .ToListAsync();

            var positive = feedbacks.FirstOrDefault(f => f.Status == true);
            var negative = feedbacks.FirstOrDefault(f => f.Status == false);

            var response = new CardFeedbacksDto
            {
                PositiveFeedback = positive != null ? new FeedbackDetailsDto
                {
                    Feedbacks_Id = positive.Feedbacks_Id,
                    Feedbacks_Long_Description = positive.Feedbacks_Long_Description
                } : null,

                NegativeFeedback = negative != null ? new FeedbackDetailsDto
                {
                    Feedbacks_Id = negative.Feedbacks_Id,
                    Feedbacks_Long_Description = negative.Feedbacks_Long_Description
                } : null
            };

            return Ok(response);
        }

        [HttpPut("feedbacks/edit")]
        public async Task<IActionResult> UpdateCardFeedbacks([FromQuery] int cardId, [FromBody] UpdateCardFeedbacksDto dto)
        {
            var card = await _context.Cards
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Card_Id == cardId);

            if (card == null)
            {
                return NotFound($"Karta o ID {cardId} nie została znaleziona.");
            }

            var positiveFeedback = await _context.Feedbacks.FirstOrDefaultAsync(f => f.Cards_Id == card.Cards_Id && f.Status == true);
            var negativeFeedback = await _context.Feedbacks.FirstOrDefaultAsync(f => f.Cards_Id == card.Cards_Id && f.Status == false);

            if (positiveFeedback == null)
            {
                positiveFeedback = new Feedback { Cards_Id = card.Cards_Id, Status = true };
                _context.Feedbacks.Add(positiveFeedback);
            }
            positiveFeedback.Feedbacks_Long_Description = dto.PositiveDescription;

            if (negativeFeedback == null)
            {
                negativeFeedback = new Feedback { Cards_Id = card.Cards_Id, Status = false };
                _context.Feedbacks.Add(negativeFeedback);
            }
            negativeFeedback.Feedbacks_Long_Description = dto.NegativeDescription;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Feedbacki zostały pomyślnie zaktualizowane." });
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
        public async Task<IActionResult> UpdateItem([FromBody] UpdateCardDto dto)
        {
            var hardware = await _context.Hardwares.FirstOrDefaultAsync(h => h.Cards.Card_Id == dto.CardId);
            if (hardware != null)
            {
                hardware.Hardwares_Short_Desc = dto.ShortDesc;
                hardware.Hardwares_Long_Desc = dto.LongDesc;
            }
            else
            {
                var software = await _context.Softwares.FirstOrDefaultAsync(s => s.Cards.Card_Id == dto.CardId);
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

        // --- Zasady ekonomii Szkolenia ---

        [HttpGet("{deckId}/economy")]
        public async Task<IActionResult> GetEconomySettings(int deckId)
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();

            var deckExists = await _context.Decks.AnyAsync(d => d.Decks_Id == deckId && d.Users_Id == userId.Value);
            if (!deckExists) return NotFound("Szkolenie nie istnieje lub brak dostępu.");

            var settings = await _economyService.GetEconomySettingsAsync(deckId);
            return Ok(settings);
        }

        [HttpPut("{deckId}/economy")]
        public async Task<IActionResult> UpdateEconomySettings(int deckId, [FromBody] UpdateDeckEconomySettingsDto dto)
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();

            var deckExists = await _context.Decks.AnyAsync(d => d.Decks_Id == deckId && d.Users_Id == userId.Value);
            if (!deckExists) return NotFound("Szkolenie nie istnieje lub brak dostępu.");

            var result = await _economyService.UpdateEconomySettingsAsync(deckId, dto);
            return Ok(result);
        }

        [HttpPost("{deckId}/economy/import")]
        public async Task<IActionResult> ImportEconomySettings(int deckId, IFormFile file)
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();

            var deckExists = await _context.Decks.AnyAsync(d => d.Decks_Id == deckId && d.Users_Id == userId.Value);
            if (!deckExists) return NotFound("Szkolenie nie istnieje lub brak dostępu.");

            try
            {
                await _provisioningService.ImportEconomySettingsFromFileAsync(deckId, file);
                return Ok(new { message = "Zasady ekonomii zostały zaimportowane pomyślnie." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Błąd: {ex.Message}" });
            }
        }

        [HttpGet("{gameId}/economy-preview")]
        public async Task<IActionResult> GetGameEconomyPreview(int gameId)
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();

            var preview = await _economyService.GetGameEconomyPreviewAsync(gameId);
            return Ok(preview);
        }
    }
}