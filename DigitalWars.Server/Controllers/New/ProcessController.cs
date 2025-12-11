using backend.Data;
using backend.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalWars.Server.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/[controller]")]
    public class ProcessController : BaseApiController
    {
        private readonly AppDbContext _context;

        public ProcessController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("create")]
        public async Task<IActionResult> AddProcess([FromBody] ProcessCreateDto data)
        {
            if (!await _context.Decks.AnyAsync(d => d.Decks_Id == data.Deck_Id)) return NotFound(CreateError("DECK_NOT_FOUND", "deckNotFound"));
            var newProcess = new Process { Processes_Desc = data.Process_Desc, Processes_Long_Desc = data.Process_Long_Desc, Processes_Color = data.Process_Color, Processes_Weight = data.Process_Weight, Decks_Id = data.Deck_Id, Modules_Id = null };
            _context.Processes.Add(newProcess);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Proces dodany.", ProcessId = newProcess.Processes_Id });
        }

        [HttpGet("read")]
        public async Task<IActionResult> GetProcessesByDeckId([FromQuery] int deckId)
        {
            if (!await _context.Decks.AnyAsync(d => d.Decks_Id == deckId)) return NotFound(CreateError("DECK_NOT_FOUND", "deckNotFound"));
            return Ok(await _context.Processes.AsNoTracking().Where(p => p.Decks_Id == deckId)
                .Select(p => new { ProcessId = p.Processes_Id, ProcessDesc = p.Processes_Desc, ProcessLongDesc = p.Processes_Long_Desc, ProcessColor = p.Processes_Color }).ToListAsync());
        }

        [HttpPut("update")]
        public async Task<IActionResult> EditProcess([FromQuery] int processId, [FromBody] ProcessEditDto data)
        {
            var process = await _context.Processes.FindAsync(processId);
            if (process == null) return NotFound(CreateError("PROCESS_NOT_FOUND", "processNotFound"));
            process.Processes_Desc = data.Process_Desc; process.Processes_Long_Desc = data.Process_Long_Desc; process.Processes_Color = data.Process_Color;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Proces zaktualizowany." });
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteProcess([FromQuery] int processId)
        {
            var process = await _context.Processes.FindAsync(processId);
            if (process == null) return NotFound(CreateError("PROCESS_NOT_FOUND", "processNotFound"));
            _context.Processes.Remove(process);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Proces usunięty." });
        }
    }
}