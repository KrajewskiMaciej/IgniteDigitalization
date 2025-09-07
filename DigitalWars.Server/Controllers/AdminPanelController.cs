using backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminPanelController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminPanelController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("teams/by-game/{gameId}")]
        public async Task<IActionResult> GetTeamsByGame(int gameId)
        {
            var teams = await _context.Teams
                .AsNoTracking()
                .Where(t => t.Games_Id == gameId)
                .Select(t => new { id = t.Teams_Id, name = t.Teams_Name, color = t.Teams_Color, token = t.Teams_Token })
                .ToListAsync();

            return Ok(teams);
        }

        [HttpGet("games/{gameId}")]
        public async Task<IActionResult> GetGameById(int gameId)
        {
            var game = await _context.Games
                .AsNoTracking()
                .Where(g => g.Games_Id == gameId)
                .Select(g => new { id = g.Games_Id, Status = g.Game_Status.ToString() })
                .FirstOrDefaultAsync();

            if (game == null) return NotFound(new { message = "Gra nie została znaleziona." });

            return Ok(game);
        }
    }
}