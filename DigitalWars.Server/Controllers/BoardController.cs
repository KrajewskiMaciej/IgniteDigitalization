using backend.Dtos;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/board")]
    public class BoardController : BaseApiController
    {
        private readonly IBoardService _boardService;

        public BoardController(IBoardService boardService)
        {
            _boardService = boardService;
        }

        [HttpGet("get")]
        public async Task<IActionResult> GetBoardsForUser()
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();

            var boards = await _boardService.GetBoardsForUserAsync(userId.Value);
            return Ok(boards);
        }

        [HttpPost("add")]
        public async Task<IActionResult> CreateBoard([FromBody] BoardCreateDto boardDto)
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();

            var newBoard = await _boardService.CreateBoardAsync(boardDto, userId.Value);
            return Ok(newBoard);
        }

        [HttpPut("edit/{id}")]
        public async Task<IActionResult> UpdateBoard(int id, [FromBody] BoardCreateDto boardDto)
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();

            try
            {
                var updatedBoard = await _boardService.UpdateBoardAsync(id, boardDto, userId.Value);
                return Ok(updatedBoard);
            }
            catch (Exception ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteBoard(int id)
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();

            try
            {
                await _boardService.DeleteBoardAsync(id, userId.Value);
                return NoContent(); // HTTP 204
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23503")
                {
                    // Zwróć błąd 409 Conflict z czytelnym komunikatem
                    return Conflict(new { message = "Nie można usunąć tej planszy, ponieważ jest ona wciąż używana przez co najmniej jedną grę.", errorCode = 1000 });
                }

                // Jeśli to inny błąd bazy danych, zwróć generyczny błąd 500
                return StatusCode(500, "Wystąpił wewnętrzny błąd serwera podczas usuwania danych.");
            }
            catch (Exception ex)
            {
                // Złap inne błędy z serwisu (np. "nie znaleziono")
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}