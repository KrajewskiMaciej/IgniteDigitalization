using backend.Data;
using DigitalWars.Server.Dtos;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using backend.Controllers;

namespace DigitalWars.Server.Services
{
    public interface IGameService
    {
        Task<Game> CreateNewGameAsync(CreateGameDto gameDto, int userId);
    }

    public class GameService : IGameService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GameService> _logger;

        public GameService(AppDbContext context, ILogger<GameService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Game> CreateNewGameAsync(CreateGameDto gameDto, int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new Exception("Użytkownik nie został znaleziony.");
            if (user.Licenses_Owned <= 0) throw new Exception("Brak dostępnych licencji.");

            // KROK 1: Weryfikacja, czy plansze istnieją w bazie danych
            var teamBoardExists = await _context.Boards.AnyAsync(b => b.Boards_Id == gameDto.BoardId && b.Users_Id == userId);
            var rivalBoardExists = await _context.Boards.AnyAsync(b => b.Boards_Id == gameDto.RivalBoardId && b.Users_Id == userId);

            if (!teamBoardExists || !rivalBoardExists)
            {
                throw new Exception("Jedna lub obie wybrane plansze są nieprawidłowe lub nie należą do tego użytkownika.");
            }

            var requestedProcessShortNames = gameDto.Processes.Select(p => p.ShortName).ToList();
            var processesFromDb = await _context.Processes
                .Where(p => p.Decks_Id == gameDto.DeckId && requestedProcessShortNames.Contains(p.Processes_Desc))
                .ToDictionaryAsync(p => p.Processes_Desc);

            if (processesFromDb.Count != requestedProcessShortNames.Count)
            {
                throw new Exception("Jeden lub więcej wybranych procesów nie istnieje w podanej talii kart.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var newGame = new Game
                {
                    Games_Desc = gameDto.GameName,
                    Teams_Boards_Id = gameDto.BoardId,
                    Rivals_Boards_Id = gameDto.RivalBoardId,
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
                        Teams_Bud = gameDto.StartBits,
                        Teams_Token = GameController.TokenGenerator.GenerateRandomAlphanumericToken(6),
                        Is_Independent = teamDto.IsAbleToMakeDecisions,
                        Games = newGame
                    };

                    foreach (var processDto in gameDto.Processes)
                    {
                        var dbProcess = processesFromDb[processDto.ShortName];
                        var newGameProcess = new GameProcess
                        {
                            Games = newGame,
                            Teams = gameTeam,
                            Processes_Id = dbProcess.Processes_Id
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
                        _context.GameBoards.Add(new GameBoard { Games_Id = newGame.Games_Id, Teams_Id = team.Teams_Id, Boards_Id = newGame.Teams_Boards_Id, Games_Processes_Id = gameProcess.Games_Processes_Id, Poz_X = 0, Poz_Y = 0 });
                    }
                    _context.GameBoards.Add(new GameBoard { Games_Id = newGame.Games_Id, Teams_Id = team.Teams_Id, Boards_Id = newGame.Rivals_Boards_Id, Games_Processes_Id = null, Poz_X = 0, Poz_Y = 0 });
                }

                // POPRAWKA: Aktualizuj tylko jedno pole, a nie cały obiekt użytkownika
                user.Licenses_Owned--;
                user.Games_In_Progress++;
                _context.Users.Attach(user);
                _context.Entry(user).Property(x => x.Licenses_Owned).IsModified = true;
                _context.Entry(user).Property(x => x.Games_In_Progress).IsModified = true;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

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