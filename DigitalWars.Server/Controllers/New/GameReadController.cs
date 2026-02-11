using backend.Data;
using DigitalWars.Server.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;

namespace DigitalWars.Server.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/[controller]")]
    public class GameReadController : BaseApiController
    {
        private readonly AppDbContext _context;
        public GameReadController(AppDbContext context) { _context = context; }

        [HttpGet("details")]
        public async Task<IActionResult> GetGameById([FromQuery] int id)
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();
            var game = await _context.Games.Where(g => g.Games_Id == id && g.Users_Id == userId).Select(g => new { Id = g.Games_Id, Name = g.Games_Desc, Status = g.Game_Status.ToString(), DeckId = g.Decks_Id }).FirstOrDefaultAsync();
            if (game == null) return NotFound(CreateError("GAME_NOT_FOUND", "gameNotFound"));
            return Ok(game);
        }

        [HttpGet("all-active")]
        public async Task<ActionResult> GetActiveGames()
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();
            return Ok(await _context.Games.Where(g => g.Users_Id == userId.Value && (g.Game_Status == GameStatus.During || g.Game_Status == GameStatus.Paused)).Select(g => new GameListItemDto { Id = g.Games_Id, Name = g.Games_Desc, Status = g.Game_Status.ToString() ?? "" }).ToListAsync());
        }

        [HttpGet("teams-basic")]
        public async Task<IActionResult> GetTeamsByGame([FromQuery] int gameId)
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();
            // Opcjonalnie: sprawdź czy gra należy do Admina
            return Ok(await _context.Teams.AsNoTracking().Where(t => t.Games_Id == gameId).Select(t => new { id = t.Teams_Id, name = t.Teams_Name, color = t.Teams_Color, token = t.Teams_Token }).ToListAsync());
        }

        [HttpGet("teams-management")]
        public async Task<IActionResult> GetTeamsManagement([FromQuery] int gameId)
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();
            return Ok(await _context.Teams.Where(t => t.Games_Id == gameId).Select(t => new TeamManagementDto { TeamId = t.Teams_Id, TeamName = t.Teams_Name, TeamBud = t.Teams_Bud, TeamToken = t.Teams_Token, TeamColor = t.Teams_Color, DeckId = t.Games.Decks_Id, BoardId = t.Games.Teams_Boards_Id }).ToListAsync());
        }
    }
}