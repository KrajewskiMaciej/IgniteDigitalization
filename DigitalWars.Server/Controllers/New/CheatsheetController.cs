using Microsoft.AspNetCore.Mvc;
using backend.Services;

namespace DigitalWars.Server.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/[controller]")]
    public class CheatsheetController : BaseApiController
    {
        private readonly ICheatsheetService _cheatsheetService;

        public CheatsheetController(ICheatsheetService cheatsheetService)
        {
            _cheatsheetService = cheatsheetService;
        }

        [HttpGet("enablers")]
        public async Task<IActionResult> GetEnablersMap([FromQuery] int deckId, [FromQuery] int? moduleId)
        {
            var cheatsheetData = await _cheatsheetService.GetEnablersMapAsync(deckId, moduleId);
            return Ok(cheatsheetData);
        }

        [HttpGet("latest-entries")]
        public async Task<IActionResult> GetLatestEntries([FromQuery] int gameId, [FromQuery] int? teamId = null)
        {
            var result = await _cheatsheetService.GetLatestEntriesAsync(gameId, teamId);
            if (teamId.HasValue && (result is int latestCardId) && latestCardId == 0)
            {
                return NotFound(CreateError("NO_ENTRIES", "noEntriesFound"));
            }
            return Ok(result);
        }

        [HttpPut("edit-enablers")]
        public async Task<IActionResult> EditEnablers([FromQuery] int cardId, [FromBody] List<int> enablerIds)
        {
            await _cheatsheetService.EditEnablersForCard(cardId, enablerIds);
            return Ok(new { message = "Zaktualizowano." });
        }
    }
}