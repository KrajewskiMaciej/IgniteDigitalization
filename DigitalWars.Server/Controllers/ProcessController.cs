using backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using backend.Dtos;

namespace backend.Controllers
{
    [Authorize]
    [Route("api/processes")]
    [ApiController]
    public class ProcessController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProcessController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("by-deck/{deckId}")]
        public async Task<IActionResult> GetProcessesByDeckId(int deckId)
        {
            if (!await _context.Decks.AnyAsync(d => d.Decks_Id == deckId))
            {
                return NotFound($"Nie znaleziono talii o ID: {deckId}");
            }

            var processes = await _context.Processes
                .AsNoTracking()
                .Where(p => p.Decks_Id == deckId)
                .Select(p => new
                {
                    ProcessId = p.Processes_Id,
                    ProcessDesc = p.Processes_Desc,
                    ProcessLongDesc = p.Processes_Long_Desc,
                    ProcessColor = p.Processes_Color
                })
                .ToListAsync();

            return Ok(processes);
        }

        [HttpPut("edit/{processId}")]
        public async Task<IActionResult> EditProcess(int processId, [FromBody] ProcessEditDto data)
        {
            
            var process = await _context.Processes.FirstOrDefaultAsync(p => p.Processes_Id == processId);
            if (process == null)
            {
                return NotFound($"Nie znaleziono procesu o ID: {processId}");
            }

            process.Processes_Desc = data.Process_Desc;
            process.Processes_Long_Desc = data.Process_Long_Desc;
            process.Processes_Color = data.Process_Color;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Proces został pomyślnie zaktualizowany." });
        }

        [HttpDelete("delete/{processId}")]
        public async Task<IActionResult> DeleteProcess(int processId)
        {
            var process = await _context.Processes.FirstOrDefaultAsync(p => p.Processes_Id == processId);
            if (process == null)
            {
                return NotFound($"Nie znaleziono procesu o ID: {processId}");
            }

            _context.Processes.Remove(process);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Proces został pomyślnie usunięty." });
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddProcess([FromBody] ProcessCreateDto data)
        {
            if (!await _context.Decks.AnyAsync(d => d.Decks_Id == data.Deck_Id))
            {
                return NotFound($"Nie znaleziono talii o ID: {data.Deck_Id}");
            }

            var newProcess = new Process
            {
                Processes_Desc = data.Process_Desc,
                Processes_Long_Desc = data.Process_Long_Desc,
                Processes_Color = data.Process_Color,
                Processes_Weight = data.Process_Weight,
                Decks_Id = data.Deck_Id,
                Modules_Id = null // tu dałem null, bo nie mamy obsługi modułów na razie

            };

            _context.Processes.Add(newProcess);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Proces został pomyślnie dodany.", ProcessId = newProcess.Processes_Id });
        }

        
    }
}