using backend.Data;
using DigitalWars.Server.Dtos;
using DigitalWars.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalWars.Server.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/[controller]")]
    public class GameController : BaseApiController
    {
        private readonly AppDbContext _context;
        private readonly IGameService _gameService;

        public GameController(AppDbContext context, IGameService gameService)
        {
            _context = context;
            _gameService = gameService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateGame([FromBody] CreateGameDto gameDto)
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();

            try
            {
                var newGame = await _gameService.CreateNewGameAsync(gameDto, userId.Value);
                return Ok(new { gameId = newGame.Games_Id, message = "Gra utworzona pomyślnie." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, CreateError("CREATE_FAILED", ex.Message));
            }
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<GameListItemDto>>> GetActiveGames()
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();

            var activeGames = await _context.Games
                .Where(g => g.Users_Id == userId.Value && (g.Game_Status == GameStatus.During || g.Game_Status == GameStatus.Paused))
                .Select(g => new GameListItemDto { Id = g.Games_Id, Name = g.Games_Desc, Status = g.Game_Status.ToString() ?? "" })
                .ToListAsync();

            return Ok(activeGames);
        }

        [HttpGet("details")]
        public async Task<IActionResult> GetGameById([FromQuery] int id)
        {
            var userId = CurrentUserId;
            var game = await _context.Games
                .Where(g => g.Games_Id == id && g.Users_Id == userId)
                .Select(g => new { Id = g.Games_Id, Name = g.Games_Desc, Status = g.Game_Status.ToString(), DeckId = g.Decks_Id })
                .FirstOrDefaultAsync();

            if (game == null) return NotFound(CreateError("GAME_NOT_FOUND", "gameNotFound"));

            return Ok(game);
        }

        [HttpPut("status")]
        public async Task<IActionResult> UpdateGameStatus([FromQuery] int gameId, [FromBody] UpdateStatusDto dto)
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();

            var game = await _context.Games.FirstOrDefaultAsync(g => g.Games_Id == gameId);
            if (game == null) return NotFound(CreateError("GAME_NOT_FOUND", "gameNotFound"));
            if (game.Users_Id != userId.Value) return Forbid();

            if (!Enum.TryParse<GameStatus>(dto.Status, true, out var newStatus))
                return BadRequest(CreateError("INVALID_STATUS", "invalidStatus"));

            game.Game_Status = newStatus;
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Status zmieniony na '{newStatus}'.", newStatus = newStatus.ToString() });
        }

        [HttpPost("stop-all")]
        public async Task<IActionResult> StopAllGames()
        {
            var userId = CurrentUserId;
            var gamesToStop = await _context.Games.Where(g => g.Users_Id == userId && g.Game_Status == GameStatus.During).ToListAsync();
            gamesToStop.ForEach(g => g.Game_Status = GameStatus.Paused);
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Zatrzymano {gamesToStop.Count} gier." });
        }

        [HttpPost("end-all")]
        public async Task<IActionResult> EndAllGames()
        {
            var userId = CurrentUserId;
            var gamesToEnd = await _context.Games.Where(g => g.Users_Id == userId && g.Game_Status != GameStatus.End).ToListAsync();
            gamesToEnd.ForEach(g => g.Game_Status = GameStatus.End);
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Zakończono {gamesToEnd.Count} gier." });
        }
    }
}