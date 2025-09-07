using backend.Data;
using backend.DTOs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services
{
    public interface IPlayerActionService
    {
        Task<object> PlayCardAsync(int cardId, CardDataDTO cardData, bool wasSuccess);
        Task ApproveLogAsync(int logId);
        Task RejectLogAsync(int logId);
    }

    public class PlayerActionService : IPlayerActionService
    {
        private readonly AppDbContext _context;
        private readonly IPlayerService _playerPosService;
        private readonly IHubContext<GameHub> _hubContext;
        private readonly ILogger<PlayerActionService> _logger;

        public PlayerActionService(AppDbContext context, IPlayerService playerPosService, IHubContext<GameHub> hubContext, ILogger<PlayerActionService> logger)
        {
            _context = context;
            _playerPosService = playerPosService;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<object> PlayCardAsync(int cardId, CardDataDTO cardData, bool wasSuccess)
        {
            var team = await _context.Teams
                .Include(t => t.Games_Events)
                .FirstOrDefaultAsync(t => t.Teams_Id == cardData.TeamId);

            if (team == null) throw new Exception($"Drużyna o ID {cardData.TeamId} nie została znaleziona.");

            var isDecision = await _context.Decisions.AnyAsync(d => d.Cards_Id == cardId);
            var isHardware = await _context.Hardwares.AnyAsync(h => h.Cards_Id == cardId);
            var isSoftware = await _context.Softwares.AnyAsync(s => s.Cards_Id == cardId);

            bool isItem = isHardware || isSoftware;
            bool finalStatus = isItem || wasSuccess;

            double finalCost = cardData.Cost;
            if (team.Games_Events != null)
            {
                if (isDecision && team.Games_Events.Decisions_Costs_Bits_Weights.HasValue)
                {
                    finalCost *= (1 + team.Games_Events.Decisions_Costs_Bits_Weights.Value);
                }
                else if (isItem)
                {
                    finalCost *= (1 + (team.Games_Events.Hardwares_Costs_Bits_Weights ?? 0) + (team.Games_Events.Softwares_Costs_Bits_Weights ?? 0));
                }
            }

            var feedback = await _context.Feedbacks
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Cards_Id == cardId && f.Cards.Decks_Id == cardData.DeckId && f.Status == finalStatus);

            var gameLogEntry = new GameLog
            {
                Data = DateTime.UtcNow,
                Teams_Id = cardData.TeamId,
                Games_Id = cardData.GameId,
                Cards_Id = cardId,
                Boards_Id = cardData.BoardId,
                Feedbacks_Id = feedback?.Feedbacks_Id,
                Costs = finalCost,
                Status = finalStatus,
                Is_Approved = team.Is_Independent || cardData.ForceExecution ? (bool?)null : false
            };
            _context.GameLogs.Add(gameLogEntry);
            await _context.SaveChangesAsync();

            if (team.Games_Events != null && team.Turns_Left > 0)
            {
                team.Turns_Left--;
                if (team.Turns_Left == 0)
                {
                    _logger.LogInformation("Wydarzenie '{EventName}' dla drużyny {TeamId} zakończyło się.", team.Games_Events.Events_Short_Desc, team.Teams_Id);
                    team.Games_Events_Id = null;
                }
            }

            if (team.Is_Independent || cardData.ForceExecution)
            {
                await ExecuteCardEffects(gameLogEntry);
                await NotifyClients(gameLogEntry.Games_Id, "HistoryUpdated");
            }
            else
            {
                await NotifyClients(gameLogEntry.Games_Id, "PendingUpdated");
            }

            return new
            {
                message = (team.Is_Independent || cardData.ForceExecution) ? "Akcja karty została wykonana." : "Sugestia zagrania karty została wysłana.",
                newTeamBudget = team.Teams_Bud
            };
        }

        public async Task ApproveLogAsync(int logId)
        {
            var logToApprove = await _context.GameLogs.FindAsync(logId);
            if (logToApprove == null) throw new Exception("Nie znaleziono logu do zatwierdzenia.");
            if (logToApprove.Is_Approved != false) throw new Exception("Ten log nie oczekuje na zatwierdzenie.");

            await ExecuteCardEffects(logToApprove);

            await NotifyClients(logToApprove.Games_Id, "PendingUpdated", "HistoryUpdated", "BoardUpdated");
        }

        public async Task RejectLogAsync(int logId)
        {
            var logToReject = await _context.GameLogs.FindAsync(logId);
            if (logToReject == null) throw new Exception("Nie znaleziono logu do odrzucenia.");

            _context.GameLogs.Remove(logToReject);
            await _context.SaveChangesAsync();

            await NotifyClients(logToReject.Games_Id, "PendingUpdated");
        }

        private async Task ExecuteCardEffects(GameLog log)
        {
            if (log.Teams_Id == null || log.Cards_Id == null) return;

            var team = await _context.Teams
                .Include(t => t.Games_Events)
                .FirstOrDefaultAsync(t => t.Teams_Id == log.Teams_Id.Value);

            if (team == null) return;

            team.Teams_Bud -= (log.Costs ?? 0);
            log.Is_Approved = true;

            if (log.Status == true)
            {
                var cardWeights = await _context.CardWeights
                    .AsNoTracking()
                    .Where(dw => dw.Cards_Id == log.Cards_Id.Value)
                    .ToListAsync();

                if (cardWeights.Any())
                {
                    _logger.LogInformation("Znaleziono {Count} wag dla karty {CardId}. Generowanie specyfikacji ruchów.", cardWeights.Count, log.Cards_Id.Value);
                    foreach (var weight in cardWeights)
                    {
                        var gameProcess = await _context.GameProcesses
                            .FirstOrDefaultAsync(gp => gp.Games_Id == log.Games_Id && gp.Teams_Id == team.Teams_Id && gp.Processes_Id == weight.Processes_Id);

                        if (gameProcess != null)
                        {
                            var (moveX, moveY) = ApplyEventMovementBooster(weight.Weights_X, weight.Weights_Y, team.Games_Events);

                            var spec = new GameLogSpec
                            {
                                Games_Logs_Id = log.Games_Logs_Id,
                                Games_Processes_Id = gameProcess.Games_Processes_Id,
                                Moves_X = moveX,
                                Moves_Y = moveY
                            };
                            _context.GameLogSpecs.Add(spec);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await _playerPosService.SetGameProcessPosAsync(log.Games_Id, team.Teams_Id);
                await _playerPosService.SetTeamPosAsync(log.Games_Id, team.Teams_Id);
            }

            await _context.SaveChangesAsync();
        }

        private (int, int) ApplyEventMovementBooster(int baseMoveX, int baseMoveY, GameEvent? activeEvent)
        {
            if (activeEvent == null) return (baseMoveX, baseMoveY);

            double boosterX = activeEvent.Boosters_X ?? 1.0;
            double boosterY = activeEvent.Boosters_Y ?? 1.0;

            return ((int)Math.Round(baseMoveX * boosterX), (int)Math.Round(baseMoveY * boosterY));
        }

        private async Task NotifyClients(int gameId, params string[] methods)
        {
            foreach (var method in methods)
            {
                await _hubContext.Clients.Group($"game-{gameId}").SendAsync(method);
            }
        }
    }
}