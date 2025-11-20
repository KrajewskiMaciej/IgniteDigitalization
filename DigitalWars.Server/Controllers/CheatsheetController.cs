using Microsoft.AspNetCore.Mvc;
using backend.Services;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheatsheetController : ControllerBase
    {
        private readonly ICheatsheetService _cheatsheetService;

        public CheatsheetController(ICheatsheetService cheatsheetService)
        {
            _cheatsheetService = cheatsheetService;
        }

        [HttpGet("enablersMap")]
        public async Task<IActionResult> GetEnablersMap([FromQuery] int deckId, [FromQuery] int? moduleId)
        {
            // Wywołujemy naszą nową, potężną metodę
            var cheatsheetData = await _cheatsheetService.GetEnablersMapAsync(deckId, moduleId);
            return Ok(cheatsheetData);
        }

        [HttpGet("latestEntries")]
        public async Task<IActionResult> GetLatestEntries([FromQuery] int gameId, [FromQuery] int? teamId = null)
        {
            var result = await _cheatsheetService.GetLatestEntriesAsync(gameId, teamId);

            // Sprawdzamy specyficzny przypadek dla pojedynczego teamId, gdzie mogliśmy nic nie znaleźć
            if (teamId.HasValue && (result is int latestCardId) && latestCardId == 0)
            {
                return NotFound($"Nie znaleziono zagranych kart dla GameId: {gameId} i TeamId: {teamId.Value}.");
            }

            return Ok(result);
        }
    }
}