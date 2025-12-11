using backend.Dtos;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace DigitalWars.Server.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/[controller]")]
    public class BoardController : BaseApiController
    {
        private readonly IBoardService _boardService;

        public BoardController(IBoardService boardService)
        {
            _boardService = boardService;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetBoards()
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();

            var boards = await _boardService.GetBoardsForUserAsync(userId.Value);
            return Ok(boards);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateBoard([FromBody] BoardCreateDto boardDto)
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();

            var newBoard = await _boardService.CreateBoardAsync(boardDto, userId.Value);
            return Ok(newBoard);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateBoard([FromQuery] int id, [FromBody] BoardCreateDto boardDto)
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

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteBoard([FromQuery] int id)
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();

            try
            {
                await _boardService.DeleteBoardAsync(id, userId.Value);
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mySqlEx && mySqlEx.Number == 1451)
                {
                    return Conflict(CreateError("DELETE_BLOCKED", "deleteBlockedByUsage"));
                }
                return StatusCode(500, CreateError("DB_ERROR", "databaseError"));
            }
            catch
            {
                return BadRequest(CreateError("DELETE_FAILED", "deleteFailed"));
            }
        }
    }
}