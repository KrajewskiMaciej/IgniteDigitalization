using backend.Data;
using backend.DTOs;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using backend.Controllers;

namespace backend.Services
{
    public interface IGameService
    {
        Task<Game> CreateNewGameAsync(CreateGameDto gameDto, int userId);
    }

    public class GameService : IGameService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GameService> _logger;
        private readonly IEconomyService _economyService;

        public GameService(AppDbContext context, ILogger<GameService> logger, IEconomyService economyService)
        {
            _context = context;
            _logger = logger;
            _economyService = economyService;
        }

        public async Task<Game> CreateNewGameAsync(CreateGameDto gameDto, int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new Exception("Użytkownik nie został znaleziony.");
            if (user.Licenses_Owned <= 0) throw new Exception("Brak dostępnych licencji.");

            // Pobierz Szkolenie z domyślnymi planszami
            var deck = await _context.Decks
                .Include(d => d.EconomySettings)
                .FirstOrDefaultAsync(d => d.Decks_Id == gameDto.DeckId);
            if (deck == null)
                throw new Exception($"Szkolenie o ID {gameDto.DeckId} nie zostało znalezione.");

            // Wyznacz plansze – ze DTO lub z domyślnych Szkolenia
            int teamBoardId = gameDto.BoardId ?? deck.Default_Teams_Boards_Id
                ?? throw new Exception("Nie wybrano planszy drużynowej i Szkolenie nie ma domyślnej planszy drużynowej.");
            int rivalBoardId = gameDto.RivalBoardId ?? deck.Default_Rivals_Boards_Id
                ?? throw new Exception("Nie wybrano planszy rywali i Szkolenie nie ma domyślnej planszy rynkowej.");

            // Weryfikacja plansz
            var teamBoardExists = await _context.Boards.AnyAsync(b => b.Boards_Id == teamBoardId && b.Users_Id == userId);
            var rivalBoardExists = await _context.Boards.AnyAsync(b => b.Boards_Id == rivalBoardId && b.Users_Id == userId);
            if (!teamBoardExists || !rivalBoardExists)
                throw new Exception("Jedna lub obie wybrane plansze są nieprawidłowe lub nie należą do tego użytkownika.");

            // Pobierz WSZYSTKIE procesy z Szkolenia
            var allProcesses = await _context.Processes
                .Where(p => p.Decks_Id == gameDto.DeckId)
                .ToListAsync();

            if (!allProcesses.Any())
                throw new Exception("Szkolenie nie zawiera żadnych procesów. Zaimportuj plik z procesami.");

            // Wyznacz budżet startowy – ze DTO lub z zasad ekonomii
            double startBits = gameDto.StartBits > 0
                ? gameDto.StartBits
                : (deck.EconomySettings?.Map1_Starting_Budget ?? 40);

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var newGame = new Game
                {
                    Games_Desc = gameDto.GameName,
                    Teams_Boards_Id = teamBoardId,
                    Rivals_Boards_Id = rivalBoardId,
                    Decks_Id = gameDto.DeckId,
                    Game_Status = GameStatus.During,
                    Users_Id = userId,
                    Is_Online = gameDto.GameMode
                };
                _context.Games.Add(newGame);

                var newTeams = new List<Team>();
                foreach (var teamDto in gameDto.Teams)
                {
                    var gameTeam = new Team
                    {
                        Teams_Name = teamDto.Name,
                        Teams_Color = teamDto.Colour,
                        Teams_Bud = startBits,
                        Teams_Token = GameController.TokenGenerator.GenerateRandomAlphanumericToken(6),
                        Is_Independent = teamDto.IsAbleToMakeDecisions,
                        Games = newGame
                    };

                    foreach (var process in allProcesses)
                    {
                        var newGameProcess = new GameProcess
                        {
                            Games = newGame,
                            Teams = gameTeam,
                            Processes_Id = process.Processes_Id
                        };
                        gameTeam.Game_Processes.Add(newGameProcess);
                    }
                    newGame.Teams.Add(gameTeam);
                    newTeams.Add(gameTeam);
                }

                await _context.SaveChangesAsync();

                foreach (var team in newTeams)
                {
                    foreach (var gameProcess in team.Game_Processes)
                    {
                        _context.GameBoards.Add(new GameBoard
                        {
                            Games_Id = newGame.Games_Id,
                            Teams_Id = team.Teams_Id,
                            Boards_Id = newGame.Teams_Boards_Id,
                            Games_Processes_Id = gameProcess.Games_Processes_Id,
                            Poz_X = 0,
                            Poz_Y = 0
                        });
                    }
                    _context.GameBoards.Add(new GameBoard
                    {
                        Games_Id = newGame.Games_Id,
                        Teams_Id = team.Teams_Id,
                        Boards_Id = newGame.Rivals_Boards_Id,
                        Games_Processes_Id = null,
                        Poz_X = 0,
                        Poz_Y = 0
                    });
                }

                user.Licenses_Owned--;
                user.Games_In_Progress++;
                _context.Users.Attach(user);
                _context.Entry(user).Property(x => x.Licenses_Owned).IsModified = true;
                _context.Entry(user).Property(x => x.Games_In_Progress).IsModified = true;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("[GameService] Utworzono grę '{Name}' (ID: {Id}) ze Szkolenia {DeckId}, {ProcessCount} procesów, {TeamCount} drużyn, budżet: {Budget} BITS.",
                    newGame.Games_Desc, newGame.Games_Id, gameDto.DeckId, allProcesses.Count, newTeams.Count, startBits);

                return newGame;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Błąd podczas tworzenia nowej gry.");
                throw;
            }
        }
    }
}