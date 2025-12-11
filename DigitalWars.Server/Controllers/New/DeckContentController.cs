using backend.Data;
using DigitalWars.Server.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalWars.Server.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/[controller]")]
    public class DeckContentController : BaseApiController
    {
        private readonly AppDbContext _context;

        public DeckContentController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("items")]
        public async Task<IActionResult> GetItems([FromQuery] int deckId)
        {
            var hardwareItems = await _context.Hardwares
                .Include(h => h.Cards)
                .Where(h => h.Cards.Decks_Id == deckId)
                .Select(h => new { id = h.Cards.Card_Id, deckId = h.Cards.Decks_Id, shortDesc = h.Hardwares_Short_Desc, longDesc = h.Hardwares_Long_Desc, type = "Hardware" })
                .ToListAsync();

            var softwareItems = await _context.Softwares
                .Include(s => s.Cards)
                .Where(s => s.Cards.Decks_Id == deckId)
                .Select(s => new { id = s.Cards.Card_Id, deckId = s.Cards.Decks_Id, shortDesc = s.Softwares_Short_Desc, longDesc = s.Softwares_Long_Desc, type = "Software" })
                .ToListAsync();

            var allItems = hardwareItems.AsEnumerable().Concat(softwareItems).OrderBy(i => i.id);
            return Ok(allItems);
        }

        [HttpPut("items-update")]
        public async Task<IActionResult> UpdateItem([FromBody] UpdateCardDto dto)
        {
            var hardware = await _context.Hardwares.FirstOrDefaultAsync(h => h.Cards.Card_Id == dto.CardId);
            if (hardware != null)
            {
                hardware.Hardwares_Short_Desc = dto.ShortDesc;
                hardware.Hardwares_Long_Desc = dto.LongDesc;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Zaktualizowano." });
            }

            var software = await _context.Softwares.FirstOrDefaultAsync(s => s.Cards.Card_Id == dto.CardId);
            if (software != null)
            {
                software.Softwares_Short_Desc = dto.ShortDesc;
                software.Softwares_Long_Desc = dto.LongDesc;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Zaktualizowano." });
            }

            return NotFound(CreateError("ITEM_NOT_FOUND", "itemNotFound"));
        }

        [HttpGet("feedbacks")]
        public async Task<IActionResult> GetFeedbacks([FromQuery] int cardId)
        {
            var feedbacks = await _context.Feedbacks
                .Include(f => f.Cards)
                .Where(f => f.Cards.Card_Id == cardId)
                .ToListAsync();
            return Ok(feedbacks);
        }

        [HttpPut("feedbacks-update")]
        public async Task<IActionResult> UpdateFeedbacks([FromQuery] int cardId, [FromBody] UpdateCardFeedbacksDto dto)
        {
            var card = await _context.Cards.FirstOrDefaultAsync(c => c.Card_Id == cardId);
            if (card == null) return NotFound(CreateError("CARD_NOT_FOUND", "cardNotFound"));

            var positive = await _context.Feedbacks.FirstOrDefaultAsync(f => f.Cards_Id == card.Cards_Id && f.Status == true);
            var negative = await _context.Feedbacks.FirstOrDefaultAsync(f => f.Cards_Id == card.Cards_Id && f.Status == false);

            if (positive == null) { positive = new Feedback { Cards_Id = card.Cards_Id, Status = true }; _context.Feedbacks.Add(positive); }
            positive.Feedbacks_Long_Description = dto.PositiveDescription;

            if (negative == null) { negative = new Feedback { Cards_Id = card.Cards_Id, Status = false }; _context.Feedbacks.Add(negative); }
            negative.Feedbacks_Long_Description = dto.NegativeDescription;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Feedback zaktualizowany." });
        }
    }
}