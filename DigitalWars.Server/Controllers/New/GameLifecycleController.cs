using backend.Data;
using DigitalWars.Server.Services;
using DigitalWars.Server.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace DigitalWars.Server.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/[controller]")]
    [Authorize(Policy = "AdminOnly")]
    public class GameLifecycleController : BaseApiController
    {
        private readonly AppDbContext _context;
        private readonly IGameService _gameService;

        public GameLifecycleController(AppDbContext context, IGameService gameService)
        {
            _context = context;
            _gameService = gameService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateGame([FromBody] CreateGameDto gameDto)
        {
            var userId = CurrentUserId!.Value;

            try
            {
                var newGame = await _gameService.CreateNewGameAsync(gameDto, userId);
                return Ok(new { gameId = newGame.Games_Id, message = "Gra utworzona pomyślnie." });
            }
            catch (Exception ex) { return StatusCode(500, CreateError("CREATE_FAILED", ex.Message)); }
        }

        [HttpPut("status-update")]
        public async Task<IActionResult> UpdateGameStatus([FromQuery] int gameId, [FromBody] UpdateStatusDto dto)
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();

            var game = await _context.Games
                .Include(g => g.Users)
                .FirstOrDefaultAsync(g => g.Games_Id == gameId);

            if (game == null) return NotFound(CreateError("GAME_NOT_FOUND", "gameNotFound"));
            if (game.Users_Id != userId.Value) return Forbid();

            if (!Enum.TryParse<GameStatus>(dto.Status, true, out var newStatus)) return BadRequest(CreateError("INVALID_STATUS", "invalidStatus"));

            if (newStatus == GameStatus.End && game.Game_Status != GameStatus.End)
            {
                if (game.Users != null)
                {
                    game.Users.Licenses_Used++;
                }
            }

            game.Game_Status = newStatus;
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Status: {newStatus}." });
        }

        [HttpPost("all-pause")]
        public async Task<IActionResult> StopAllGames()
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();
            var games = await _context.Games
                .Where(g => g.Users_Id == userId && g.Game_Status == GameStatus.During)
                .ToListAsync();

            games.ForEach(g => g.Game_Status = GameStatus.Paused);
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Zatrzymano {games.Count} gier." });
        }

        [HttpPost("all-end")]
        public async Task<IActionResult> EndAllGames()
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();

            var games = await _context.Games
                .Include(g => g.Users)
                .Where(g => g.Users_Id == userId && g.Game_Status != GameStatus.End)
                .ToListAsync();

            foreach (var game in games)
            {
                game.Game_Status = GameStatus.End;
                if (game.Users != null)
                {
                    game.Users.Licenses_Used++;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Zakończono {games.Count} gier." });
        }
    }
}