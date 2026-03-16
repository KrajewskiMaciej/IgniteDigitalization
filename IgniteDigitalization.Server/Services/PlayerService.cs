using backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.SignalR;
using backend.Hubs;

namespace backend.Services
{
    public interface IPlayerService
    {
        Task SetGameProcessPosAsync(int gameId, int teamId);
        Task SetTeamPosAsync(int gameId, int teamId);
    }
    public class PlayerService : IPlayerService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PlayerService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<GameHub> _hubContext;

        public PlayerService(AppDbContext context, IConfiguration configuration, ILogger<PlayerService> logger, IHubContext<GameHub> hubContext)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        public async Task SetGameProcessPosAsync(int gameId, int teamId)
        {
            _logger.LogInformation("Rozpoczynanie aktualizacji pozycji PIONKÓW-PROCESÓW dla GameId: {GameId}, TeamId: {TeamId}", gameId, teamId);

            // Pozycje zapisywane jako raw suma wag (bez dzielenia przez 100).
            // Frontend przelicza na % względem MaxPosX/MaxPosY obliczonego z CardWeights.
            var movesByGeneralProcessId = await _context.GameLogSpecs
                .AsNoTracking()
                .Where(gls =>
                    gls.Games_Logs.Games_Id == gameId &&
                    gls.Games_Logs.Teams_Id == teamId &&
                    gls.Games_Processes_Id.HasValue &&
                    gls.Games_Processes != null &&
                    gls.Games_Logs.Is_Approved == true &&
                    gls.Games_Logs.Status == true
                )


                .GroupBy(gls => gls.Games_Processes!.Processes_Id)
                .Select(group => new
                {
                    ProcessId = group.Key,
                    FinalPosX = group.Sum(g => g.Moves_X),
                    FinalPosY = group.Sum(g => g.Moves_Y)
                })
                .ToDictionaryAsync(p => p.ProcessId, p => new { p.FinalPosX, p.FinalPosY });

            if (!movesByGeneralProcessId.Any())
            {
                _logger.LogInformation("Brak nowych ruchów dla pionków-procesów.");
                return;
            }

            var gameBoardEntriesToUpdate = await _context.GameBoards
                .Include(gb => gb.Games_Processes)
                .Where(gb =>
                    gb.Games_Id == gameId &&
                    gb.Teams_Id == teamId &&
                    gb.Games_Processes_Id != null &&
                    gb.Games_Processes != null)
                .ToListAsync();

            foreach (var entry in gameBoardEntriesToUpdate)
            {
                if (entry.Games_Processes != null && movesByGeneralProcessId.TryGetValue(entry.Games_Processes.Processes_Id, out var newPosition))
                {
                    // Raw suma wag – bez dzielenia, bez klampowania do rozmiaru planszy
                    entry.Poz_X = Math.Max(0, (double)newPosition.FinalPosX);
                    entry.Poz_Y = Math.Max(0, (double)newPosition.FinalPosY);
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Zakończono aktualizację pozycji pionków-procesów.");

            await _hubContext.Clients.Group($"game-{gameId}").SendAsync("BoardUpdated");
        }

        public async Task SetTeamPosAsync(int gameId, int teamId)
        {
            // Pobierz pozycje wszystkich pionków-procesów danej drużyny w danej grze.
            // Pozycja drużyny = średnia arytmetyczna pozycji procesów (bez ważenia).
            var processPawns = await _context.GameBoards
                .AsNoTracking()
                .Where(gb =>
                    gb.Games_Id == gameId &&
                    gb.Teams_Id == teamId &&
                    gb.Games_Processes_Id != null &&
                    gb.Games_Processes != null)
                .Select(gb => new { PosX = gb.Poz_X, PosY = gb.Poz_Y })
                .ToListAsync();

            if (!processPawns.Any())
            {
                _logger.LogWarning("Nie znaleziono pionków-procesów dla drużyny {TeamId} w grze {GameId}. Nie można obliczyć pozycji drużyny.", teamId, gameId);
                return;
            }

            // Średnia arytmetyczna – każdy proces ma jednakową wagę
            double avgX = processPawns.Average(p => p.PosX);
            double avgY = processPawns.Average(p => p.PosY);

            // Znajdź główny pionek drużyny (Games_Processes_Id == null)
            var teamPawnEntry = await _context.GameBoards
                .FirstOrDefaultAsync(gb => gb.Games_Id == gameId && gb.Teams_Id == teamId && gb.Games_Processes_Id == null);

            if (teamPawnEntry == null)
            {
                _logger.LogError("BŁĄD KRYTYCZNY: Nie znaleziono głównego pionka dla drużyny {TeamId} w grze {GameId}.", teamId, gameId);
                return;
            }

            // Zapisujemy wartość raw (bez klampowania do rozmiaru planszy).
            // Frontend przelicza na % względem MaxPosX/MaxPosY obliczonego z CardWeights.
            teamPawnEntry.Poz_X = Math.Max(0, avgX);
            teamPawnEntry.Poz_Y = Math.Max(0, avgY);

            await _context.SaveChangesAsync();
            _logger.LogInformation("Zaktualizowano średnią arytmetyczną pozycję ({PosX}, {PosY}) dla drużyny {TeamId} w grze {GameId}.", avgX, avgY, teamId, gameId);

            await _hubContext.Clients.Group($"game-{gameId}").SendAsync("BoardUpdated");
        }

    }
}