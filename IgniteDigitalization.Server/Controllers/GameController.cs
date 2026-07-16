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
    // DTO for status updates
    public class UpdateStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }

    [Route("api/games")]
    public class GameController : BaseApiController
    {
        private readonly AppDbContext _context;
        private readonly IGameService _gameService;
        private readonly IEconomyService _economyService;

        public GameController(AppDbContext context, IGameService gameService, IEconomyService economyService)
        {
            _context = context;
            _gameService = gameService;
            _economyService = economyService;
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

            var activeGames = (await _context.Games
                .Where(g => g.Users_Id == userId.Value && (g.Game_Status == GameStatus.During || g.Game_Status == GameStatus.Paused))
                .Select(g => new { g.Games_Id, g.Games_Desc, g.Game_Status, DeckName = g.Decks.Deck_Name })
                .ToListAsync())
                .Select(g => new GameListItemDto { Id = g.Games_Id, Name = g.Games_Desc, Status = g.Game_Status.ToString() ?? "", DeckName = g.DeckName })
                .ToList();

            return Ok(activeGames);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGameById(int id)
        {
            var userId = CurrentUserId;
            var raw = await _context.Games
                .Where(g => g.Games_Id == id && g.Users_Id == userId)
                .Select(g => new { g.Games_Id, g.Games_Desc, g.Game_Status, g.Decks_Id, DeckName = g.Decks.Deck_Name })
                .FirstOrDefaultAsync();

            if (raw == null) return NotFound();

            var game = new { Id = raw.Games_Id, Name = raw.Games_Desc, Status = raw.Game_Status.ToString(), DeckId = raw.Decks_Id, DeckName = raw.DeckName };

            return Ok(game);
        }

        [Authorize]
        [HttpPut("{gameId}/status")]
        public async Task<IActionResult> UpdateGameStatus(int gameId, [FromBody] UpdateStatusDto dto)
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized("Nie można zidentyfikować użytkownika.");

            var game = await _context.Games.FirstOrDefaultAsync(g => g.Games_Id == gameId);
            if (game == null) return NotFound($"Gra o ID {gameId} nie została znaleziona.");
            if (game.Users_Id != userId.Value) return Forbid("Nie masz uprawnień do zmiany statusu tej gry.");

            if (!Enum.TryParse<GameStatus>(dto.Status, true, out var newStatus))
                return BadRequest("Nieprawidłowa wartość statusu.");

            if (game.Game_Status == GameStatus.End && newStatus != GameStatus.End)
                return BadRequest("Nie można zmienić statusu zakończonej gry.");

            // Zakończenie gry zdejmuje ją z licznika gier w toku
            if (game.Game_Status != GameStatus.End && newStatus == GameStatus.End)
            {
                var user = await _context.Users.FindAsync(userId.Value);
                if (user != null) user.Games_In_Progress = Math.Max(0, user.Games_In_Progress - 1);
            }

            game.Game_Status = newStatus;
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Status gry pomyślnie zmieniony na '{newStatus}'.", newStatus = newStatus.ToString() });
        }

        [Authorize]
        [HttpPost("stop-all")]
        public async Task<IActionResult> StopAllGames()
        {
            var userId = CurrentUserId;
            var gamesToStop = await _context.Games
                .Where(g => g.Users_Id == userId && g.Game_Status == GameStatus.During)
                .ToListAsync();

            gamesToStop.ForEach(g => g.Game_Status = GameStatus.Paused);
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Zatrzymano {gamesToStop.Count} gier." });
        }

        [Authorize]
        [HttpPost("end-all")]
        public async Task<IActionResult> EndAllGames()
        {
            var userId = CurrentUserId;
            var gamesToEnd = await _context.Games
                .Where(g => g.Users_Id == userId && g.Game_Status != GameStatus.End)
                .ToListAsync();

            gamesToEnd.ForEach(g => g.Game_Status = GameStatus.End);

            // Wszystkie zakończone gry schodzą z licznika gier w toku
            var user = await _context.Users.FindAsync(userId);
            if (user != null) user.Games_In_Progress = Math.Max(0, user.Games_In_Progress - gamesToEnd.Count);

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Zakończono {gamesToEnd.Count} gier." });
        }

        // Helper class for token generation
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

