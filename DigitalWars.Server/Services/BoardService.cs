using backend.Data;
using backend.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services
{
    public interface IBoardService
    {
        Task<List<Board>> GetBoardsForUserAsync(int userId);
        Task<Board> CreateBoardAsync(BoardCreateDto boardDto, int userId);
        Task<Board> UpdateBoardAsync(int boardId, BoardCreateDto boardDto, int userId);
        Task DeleteBoardAsync(int boardId, int userId);
    }
    public class BoardService : IBoardService
    {
        private readonly AppDbContext _context;

        public BoardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Board>> GetBoardsForUserAsync(int userId)
        {
            return await _context.Boards
                .AsNoTracking()
                .Where(b => b.Users_Id == userId)
                .ToListAsync();
        }

        public async Task<Board> CreateBoardAsync(BoardCreateDto boardDto, int userId)
        {
            var newBoard = new Board
            {
                Users_Id = userId,
                Name = boardDto.Name,
                Labels_Up = boardDto.Labels_Up ?? string.Empty,
                Labels_Right = boardDto.Labels_Right ?? string.Empty,
                Description_Down = boardDto.Description_Down ?? string.Empty,
                Description_Left = boardDto.Description_Left ?? string.Empty,
                Rows = boardDto.Rows,
                Cols = boardDto.Cols,
                Cell_Color = boardDto.Cell_Color,
                Border_Color = boardDto.Border_Color,
                Borders_Colors = boardDto.Borders_Colors ?? string.Empty,
                Cells_Descriptions = boardDto.Cells_Descriptions ?? string.Empty
            };

            _context.Boards.Add(newBoard);
            await _context.SaveChangesAsync();
            return newBoard;
        }

        public async Task<Board> UpdateBoardAsync(int boardId, BoardCreateDto boardDto, int userId)
        {
            var boardToUpdate = await _context.Boards.FindAsync(boardId);
            if (boardToUpdate == null)
                throw new Exception($"Plansza o ID {boardId} nie została znaleziona.");

            if (boardToUpdate.Users_Id != userId)
                throw new Exception("Brak uprawnień do edycji tej planszy.");

            boardToUpdate.Name = boardDto.Name;
            boardToUpdate.Labels_Up = boardDto.Labels_Up ?? string.Empty;
            boardToUpdate.Labels_Right = boardDto.Labels_Right ?? string.Empty;
            boardToUpdate.Description_Down = boardDto.Description_Down ?? string.Empty;
            boardToUpdate.Description_Left = boardDto.Description_Left ?? string.Empty;
            boardToUpdate.Rows = boardDto.Rows;
            boardToUpdate.Cols = boardDto.Cols;
            boardToUpdate.Cell_Color = boardDto.Cell_Color;
            boardToUpdate.Border_Color = boardDto.Border_Color;
            boardToUpdate.Borders_Colors = boardDto.Borders_Colors ?? string.Empty;
            boardToUpdate.Cells_Descriptions = boardDto.Cells_Descriptions ?? string.Empty;

            await _context.SaveChangesAsync();
            return boardToUpdate;
        }

        public async Task DeleteBoardAsync(int boardId, int userId)
        {
            var boardToDelete = await _context.Boards.FindAsync(boardId);
            if (boardToDelete == null)
                throw new Exception($"Plansza o ID {boardId} nie została znaleziona.");

            if (boardToDelete.Users_Id != userId)
                throw new Exception("Brak uprawnień do usunięcia tej planszy.");

            _context.Boards.Remove(boardToDelete);
            await _context.SaveChangesAsync();
        }
    }
}