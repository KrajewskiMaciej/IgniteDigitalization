using backend.Data;
using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [Route("api/games")]
    public class GameController : BaseApiController
    {
        private readonly AppDbContext _context;
        private readonly IGameService _gameService;

        public GameController(AppDbContext context, IGameService gameService)
        {
            _context = context;
            _gameService = gameService;
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateGame([FromBody] CreateGameDto gameDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = CurrentUserId;
            if (userId == null) return Unauthorized("Nieprawidłowy identyfikator użytkownika.");

            try
            {
                var newGame = await _gameService.CreateNewGameAsync(gameDto, userId.Value);
                return Ok(new { gameId = newGame.Games_Id, message = "Gra utworzona pomyślnie." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Wystąpił błąd serwera: {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<GameListItemDto>>> GetActiveGames()
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized("Nie można zidentyfikować użytkownika.");

            var activeGames = await _context.Games
                .Where(g => g.Users_Id == userId.Value && (g.Game_Status == GameStatus.During || g.Game_Status == GameStatus.Paused))
                .Select(g => new GameListItemDto { Id = g.Games_Id, Name = g.Games_Desc, Status = g.Game_Status.ToString() })
                .ToListAsync();

            return Ok(activeGames);
        }

        [Authorize]
        [HttpPut("{gameId}/status")]
        public async Task<IActionResult> UpdateGameStatus(int gameId, [FromBody] string status)
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized("Nie można zidentyfikować użytkownika.");

            var game = await _context.Games.FirstOrDefaultAsync(g => g.Games_Id == gameId);
            if (game == null) return NotFound($"Gra o ID {gameId} nie została znaleziona.");
            if (game.Users_Id != userId.Value) return Forbid("Nie masz uprawnień do zmiany statusu tej gry.");

            if (!Enum.TryParse<GameStatus>(status, true, out var newStatus))
                return BadRequest("Nieprawidłowa wartość statusu.");

            if (game.Game_Status == GameStatus.End && newStatus != GameStatus.End)
                return BadRequest("Nie można zmienić statusu zakończonej gry.");

            game.Game_Status = newStatus;
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Status gry pomyślnie zmieniony na '{newStatus}'.", newStatus = newStatus.ToString() });
        }

        public static class TokenGenerator
        {
            private static readonly Random _random = new Random();
            public static string GenerateRandomAlphanumericToken(int length)
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                return new string(Enumerable.Repeat(chars, length)
                  .Select(s => s[_random.Next(s.Length)]).ToArray());
            }
        }
    }
}