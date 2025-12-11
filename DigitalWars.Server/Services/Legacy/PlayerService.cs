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
        private const int PositionDivisor = 100;
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

            // [KROK 1] Pobranie planszy dla pionków-procesów.
            // Upewniamy się, że pobieramy planszę z pionka, który jest procesem (ma przypisane Games_Processes_Id).
            var processBoardInfo = await _context.GameBoards
                .AsNoTracking()
                .Where(gb => gb.Games_Id == gameId && gb.Teams_Id == teamId && gb.Games_Processes_Id != null)
                .Select(gb => new { gb.Boards.Rows, gb.Boards.Cols })
                .FirstOrDefaultAsync();

            if (processBoardInfo == null)
            {
                _logger.LogWarning("Nie znaleziono planszy dla pionków-procesów w grze {GameId}. Przerywanie aktualizacji.", gameId);
                return;
            }

            double maxPozX = processBoardInfo.Cols > 0 ? processBoardInfo.Cols - 1 : 0;
            double maxPozY = processBoardInfo.Rows > 0 ? processBoardInfo.Rows - 1 : 0;

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
                    var finalX = newPosition.FinalPosX / PositionDivisor;
                    var finalY = newPosition.FinalPosY / PositionDivisor;

                    // [KROK 2] Zastosowanie ograniczeń planszy procesów
                    double clampedX = Math.Max(0, Math.Min(finalX, maxPozX));
                    double clampedY = Math.Max(0, Math.Min(finalY, maxPozY));

                    entry.Poz_X = clampedX;
                    entry.Poz_Y = clampedY;
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Zakończono aktualizację pozycji pionków-procesów.");

            await _hubContext.Clients.Group($"game-{gameId}").SendAsync("BoardUpdated");
        }

        public async Task SetTeamPosAsync(int gameId, int teamId)
        {
            var processPawns = await _context.GameBoards
                .Where(gb =>
                    gb.Games_Id == gameId &&
                    gb.Teams_Id == teamId &&
                    gb.Games_Processes_Id != null &&
                    gb.Games_Processes != null &&
                    gb.Games_Processes.Processes != null)
                .Select(gb => new PawnData
                {
                    ProcessId = gb.Games_Processes!.Processes!.Processes_Id,
                    PosX = gb.Poz_X,
                    PosY = gb.Poz_Y,
                    Weight = gb.Games_Processes.Processes.Processes_Weight
                })
                .ToListAsync();

            if (!processPawns.Any())
            {
                _logger.LogWarning("Nie znaleziono pionków-procesów dla drużyny {TeamId} w grze {GameId}. Nie można obliczyć pozycji drużyny.", teamId, gameId);
                return;
            }

            List<NormalizedPawnData> normalizedPawns = ProcessAndNormalizeWeights(processPawns);

            double weightedSumX = normalizedPawns.Sum(p => p.PosX * p.NormalizedWeight);
            double weightedSumY = normalizedPawns.Sum(p => p.PosY * p.NormalizedWeight);

            int finalAvgX = (int)Math.Round(weightedSumX);
            int finalAvgY = (int)Math.Round(weightedSumY);

            // [KROK 1] Znajdź główny pionek drużyny I JEGO PLANSZĘ
            var teamPawnEntry = await _context.GameBoards
                .Include(gb => gb.Boards) // Dołączamy powiązaną encję Board
                .FirstOrDefaultAsync(gb => gb.Games_Id == gameId && gb.Teams_Id == teamId && gb.Games_Processes_Id == null);

            if (teamPawnEntry == null)
            {
                _logger.LogError("BŁĄD KRYTYCZNY: Nie znaleziono głównego pionka dla drużyny {TeamId} w grze {GameId}.", teamId, gameId);
                return;
            }
            if (teamPawnEntry.Boards == null)
            {
                _logger.LogError("BŁĄD KRYTYCZNY: Pionek drużyny {TeamId} nie ma przypisanej planszy.", teamId);
                return;
            }

            // [KROK 2] Oblicz granice planszy DRUŻYNY
            double maxTeamPozX = teamPawnEntry.Boards.Cols > 0 ? teamPawnEntry.Boards.Cols - 1 : 0;
            double maxTeamPozY = teamPawnEntry.Boards.Rows > 0 ? teamPawnEntry.Boards.Rows - 1 : 0;

            // [KROK 3] Zastosuj ograniczenia planszy drużyny
            double clampedX = Math.Max(0, Math.Min(finalAvgX, maxTeamPozX));
            double clampedY = Math.Max(0, Math.Min(finalAvgY, maxTeamPozY));

            teamPawnEntry.Poz_X = clampedX;
            teamPawnEntry.Poz_Y = clampedY;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Zaktualizowano średnią ważoną pozycję ({PosX}, {PosY}) dla drużyny {TeamId} w grze {GameId}.", clampedX, clampedY, teamId, gameId);

            await _hubContext.Clients.Group(gameId.ToString()).SendAsync("BoardUpdated", new { teamId });
        }

        // ... reszta serwisu (ProcessAndNormalizeWeights, klasy PawnData itd.) pozostaje bez zmian ...
        private List<NormalizedPawnData> ProcessAndNormalizeWeights(List<PawnData> pawns)
        {
            var pawnsWithWeight = pawns.Where(p => p.Weight > 0).ToList();
            var pawnsWithoutWeight = pawns.Where(p => p.Weight <= 0).ToList();

            double totalWeightProvided = pawnsWithWeight.Sum(p => p.Weight);
            double missingWeight = 1.0 - totalWeightProvided;

            if (missingWeight <= 0 || !pawnsWithWeight.Any())
            {
                _logger.LogInformation("Wagi nie wymagają redystrybucji. Normalizowanie {PawnCount} pionków.", pawns.Count);
                return Normalize(pawns, pawns.Sum(p => p.Weight));
            }

            _logger.LogInformation(
                "Wykryto brakującą wagę: {MissingWeight}. Rozdzielanie jej między {PawnCount} pionków.",
                missingWeight, pawnsWithWeight.Count);

            var temporaryWeights = new Dictionary<int, double>();

            foreach (var pawn in pawnsWithWeight)
            {
                temporaryWeights[pawn.ProcessId] = pawn.Weight;
            }

            foreach (var pawn in pawnsWithWeight)
            {
                double proportion = pawn.Weight / totalWeightProvided;
                temporaryWeights[pawn.ProcessId] += missingWeight * proportion;
            }

            var finalPawnData = pawns.Select(p => new PawnData
            {
                ProcessId = p.ProcessId,
                PosX = p.PosX,
                PosY = p.PosY,
                Weight = temporaryWeights.GetValueOrDefault(p.ProcessId, 0)
            }).ToList();

            return Normalize(finalPawnData, finalPawnData.Sum(p => p.Weight));
        }

        private List<NormalizedPawnData> Normalize(List<PawnData> pawns, double totalWeight)
        {
            if (totalWeight == 0)
            {
                _logger.LogWarning("Całkowita suma wag wynosi 0, nie można znormalizować. Zwracam wagi zerowe.");
                return pawns.Select(p => new NormalizedPawnData(p, 0)).ToList();
            }

            return pawns.Select(p => new NormalizedPawnData(p, p.Weight / totalWeight)).ToList();
        }

        private class PawnData
        {
            public int ProcessId { get; set; }
            public double PosX { get; set; }
            public double PosY { get; set; }
            public double Weight { get; set; }
        }

        private class NormalizedPawnData
        {
            public int ProcessId { get; }
            public double PosX { get; }
            public double PosY { get; }
            public double NormalizedWeight { get; }

            public NormalizedPawnData(PawnData source, double normalizedWeight)
            {
                ProcessId = source.ProcessId;
                PosX = source.PosX;
                PosY = source.PosY;
                NormalizedWeight = normalizedWeight;
            }
        }
    }
}