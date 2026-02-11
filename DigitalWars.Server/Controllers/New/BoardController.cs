using backend.Dtos;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using DigitalWars.Server.PdfGeneration;
using QuestPDF.Fluent;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;

namespace DigitalWars.Server.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/[controller]")]
    [Authorize(Policy = "AdminOnly")]
    public class BoardController : BaseApiController
    {
        private readonly IBoardService _boardService;
        private readonly backend.Data.AppDbContext _context;

        public BoardController(IBoardService boardService, backend.Data.AppDbContext context)
        {
            _boardService = boardService;
            _context = context;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateBoard([FromBody] BoardCreateDto boardDto)
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();

            var newBoard = await _boardService.CreateBoardAsync(boardDto, userId.Value);
            return Ok(newBoard);
        }

        [HttpGet("read")]
        public async Task<IActionResult> GetBoards()
        {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();

            var boards = await _boardService.GetBoardsForUserAsync(userId.Value);
            return Ok(boards);
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
            catch (DbUpdateException ex) when (ex.InnerException is MySqlException sqlEx && sqlEx.Number == 1451)
            {
                return Conflict(CreateError("DELETE_BLOCKED", "deleteBlockedByUsage"));
            }
            catch { return BadRequest(CreateError("DELETE_FAILED", "deleteFailed")); }
        }

        [HttpGet("export-pdf")]
        public async Task<IActionResult> GenerateBoardsPdf([FromQuery] int teamBoardId, [FromQuery] int rivalBoardId)
        {
            var boards = await _context.Boards.AsNoTracking()
                .Where(b => b.Boards_Id == teamBoardId || b.Boards_Id == rivalBoardId).ToListAsync();

            if (boards.Count < 2) return NotFound(CreateError("BOARDS_NOT_FOUND", "boardsNotFound"));

            var document = new BoardsDocument(boards, teamBoardId, rivalBoardId);
            return File(document.GeneratePdf(), "application/pdf", "DigitalWars_Plansze.pdf");
        }
    }
}