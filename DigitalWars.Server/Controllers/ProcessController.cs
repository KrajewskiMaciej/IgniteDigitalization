using backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

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
    }
}