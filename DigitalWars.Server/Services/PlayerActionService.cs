using backend.Data;
using backend.Dtos;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services
{
    public interface IPlayerActionService
    {
        Task<object> PlayCardAsync(int cardId, CardDataDto cardData, bool wasSuccess);
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

        public async Task<object> PlayCardAsync(int cardId, CardDataDto cardData, bool wasSuccess)
        {
            var cardEntity = await _context.Cards.FirstOrDefaultAsync(c => c.Card_Id == cardId);
            if (cardEntity == null) throw new Exception($"Karta o identyfikatorze (Card_Id) {cardId} nie została znaleziona.");

            var team = await _context.Teams
                .Include(t => t.Games_Events)
                .FirstOrDefaultAsync(t => t.Teams_Id == cardData.TeamId);
            if (team == null) throw new Exception($"Drużyna o ID {cardData.TeamId} nie została znaleziona.");

            // Identyfikacja typu karty
            CardType cardType = CardType.Unknown;
            if (await _context.Decisions.AnyAsync(d => d.Cards_Id == cardEntity.Cards_Id)) cardType = CardType.Decision;
            else if (await _context.Hardwares.AnyAsync(h => h.Cards_Id == cardEntity.Cards_Id)) cardType = CardType.Hardware;
            else if (await _context.Softwares.AnyAsync(s => s.Cards_Id == cardEntity.Cards_Id)) cardType = CardType.Software;

            // Obliczenie finalnego kosztu i przechwycenie modyfikatorów
            double finalCost = cardData.Cost;
            double? eventBoosterX = null;
            double? eventBoosterY = null;

            if (team.Games_Events != null)
            {
                eventBoosterX = team.Games_Events.Boosters_X;
                eventBoosterY = team.Games_Events.Boosters_Y;

                double eventCostModifier = 0.0;
                switch (cardType)
                {
                    case CardType.Decision:
                        eventCostModifier = team.Games_Events.Decisions_Costs_Bits_Weights ?? 0;
                        break;
                    case CardType.Hardware:
                        eventCostModifier = team.Games_Events.Hardwares_Costs_Bits_Weights ?? 0;
                        break;
                    case CardType.Software:
                        eventCostModifier = team.Games_Events.Softwares_Costs_Bits_Weights ?? 0;
                        break;
                }
                finalCost *= (1 + eventCostModifier);

            }

            bool isItem = cardType == CardType.Hardware || cardType == CardType.Software;
            bool finalStatus = isItem || wasSuccess;

            var feedback = await _context.Feedbacks
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Cards_Id == cardEntity.Cards_Id && f.Cards.Decks_Id == cardData.DeckId && f.Status == finalStatus);

            var gameLogEntry = new GameLog
            {
                Data = DateTime.UtcNow,
                Teams_Id = cardData.TeamId,
                Games_Id = cardData.GameId,
                Cards_Id = cardEntity.Cards_Id,
                Boards_Id = cardData.BoardId,
                Feedbacks_Id = feedback?.Feedbacks_Id,
                Costs = finalCost,
                Status = finalStatus,
                Is_Approved = team.Is_Independent || cardData.ForceExecution ? (bool?)true : false,

                // Zapisujemy "zamrożone" wartości boosterów
                Booster_X = eventBoosterX,
                Booster_Y = eventBoosterY
            };
            _context.GameLogs.Add(gameLogEntry);

            // ... tutaj logika związana z tworzeniem GameLogSpec, jeśli jest potrzebna
            // np. na podstawie definicji karty, dodajesz do gameLogEntry.GameLogSpecs

            if (team.Games_Events != null && team.Turns_Left > 0)
            {
                team.Turns_Left--;
                if (team.Turns_Left == 0)
                {
                    _logger.LogInformation("Wydarzenie '{EventName}' dla drużyny {TeamId} zakończyło się.", team.Games_Events.Events_Short_Desc, team.Teams_Id);
                    team.Games_Events_Id = null;
                }
            }

            await _context.SaveChangesAsync();

            if (gameLogEntry.Is_Approved == true)
            {
                await ExecuteCardEffects(gameLogEntry);
                await NotifyAdmin(gameLogEntry.Games_Id, "HistoryUpdated");
                await NotifyTeam(gameLogEntry.Games_Id, cardData.TeamId, "HistoryUpdated", "PendingUpdated", "BoardUpdated");
            }
            else
            {
                await NotifyAdmin(gameLogEntry.Games_Id, "PendingUpdated");
                await NotifyTeam(gameLogEntry.Games_Id, cardData.TeamId, "PendingUpdated");
            }

            return new
            {
                message = (gameLogEntry.Is_Approved == true) ? "Akcja karty została wykonana." : "Sugestia zagrania karty została wysłana.",
                newTeamBudget = team.Teams_Bud
            };
        }

        public async Task ApproveLogAsync(int logId)
        {
            var logToApprove = await _context.GameLogs.FindAsync(logId);
            if (logToApprove == null) throw new Exception("Nie znaleziono logu do zatwierdzenia.");
            if (logToApprove.Is_Approved != false) throw new Exception("Ten log nie oczekuje na zatwierdzenie.");

            logToApprove.Is_Approved = true;

            await ExecuteCardEffects(logToApprove);

            if (logToApprove.Teams_Id == null)
            {
                throw new Exception("Log do zatwierdzenia nie ma przypisanej drużyny.");
            }

             if (logToApprove.Games_Id == null)
            {
                throw new Exception("Log do zatwierdzenia nie ma przypisanego gry.");
            }


            await NotifyAdmin(logToApprove.Games_Id, "PendingUpdated", "HistoryUpdated", "BoardUpdated");
            await NotifyTeam(logToApprove.Games_Id, logToApprove.Teams_Id, "PendingUpdated", "HistoryUpdated", "BoardUpdated");
        }

        public async Task RejectLogAsync(int logId)
        {
            var logToReject = await _context.GameLogs.FindAsync(logId);
            if (logToReject == null) throw new Exception("Nie znaleziono logu do odrzucenia.");

            _context.GameLogs.Remove(logToReject);
            await _context.SaveChangesAsync();

            await NotifyAdmin(logToReject.Games_Id, "PendingUpdated");
            await NotifyTeam(logToReject.Games_Id, logToReject.Teams_Id, "PendingUpdated");
        }

        private async Task ExecuteCardEffects(GameLog gameLogEntry)
        {
            // Upewnij się, że specyfikacje są załadowane
            var logEntryWithSpecs = await _context.GameLogs
                .Include(gl => gl.GameLogSpecs)
                .FirstOrDefaultAsync(gl => gl.Games_Logs_Id == gameLogEntry.Games_Logs_Id);

            if (logEntryWithSpecs == null) return;

            var team = await _context.Teams.FindAsync(logEntryWithSpecs.Teams_Id);
            if (team == null) return;

            // Pobierz "zamrożone" modyfikatory z wydarzenia
            // Jeśli booster jest nullem, przyjmujemy 0 (modyfikator nie zmienia wartości)
            double boosterX = logEntryWithSpecs.Booster_X ?? 0;
            double boosterY = logEntryWithSpecs.Booster_Y ?? 0;

            // Zastosuj efekty zdefiniowane w każdej specyfikacji logu
            foreach (var spec in logEntryWithSpecs.GameLogSpecs)
            {
                // 1. Pobierz bazowe efekty z GameLogSpec
                double baseMoveX = spec.Moves_X;
                double baseMoveY = spec.Moves_Y;

                // 2. Oblicz finalny efekt, uwzględniając boostery
                //    Zakładamy, że boostery to mnożniki. Jeśli mają być wartościami dodawanymi, zmień `* (1 + booster)` na `+ booster`.
                double finalMoveX = baseMoveX * (1 + boosterX);
                double finalMoveY = baseMoveY * (1 + boosterY);

                // 3. Zastosuj finalny, zmodyfikowany efekt na drużynie
                //    (to jest przykład, dostosuj do swoich statystyk drużyny)
                // team.SomeStatX += (int)Math.Round(finalMoveX);
                // team.SomeStatY += (int)Math.Round(finalMoveY);

                _logger.LogInformation(
                    "Dla drużyny {TeamId} zastosowano efekt (Spec ID: {SpecId}): Zmiana X o {FinalX} (Baza: {BaseX}, Mnożnik z wydarzenia: {ModX}%)",
                    team.Teams_Id, spec.Games_Logs_Specs_Id, finalMoveX, baseMoveX, boosterX * 100
                );
            }

            // Na koniec potrąć finalny, przeliczony koszt z budżetu
            team.Teams_Bud -= logEntryWithSpecs.Costs ?? 0;

            await _context.SaveChangesAsync();
        }

        private (int, int) ApplyEventMovementBooster(int baseMoveX, int baseMoveY, GameEvent? activeEvent)
        {
            if (activeEvent == null) return (baseMoveX, baseMoveY);

            double boosterX = activeEvent.Boosters_X ?? 1.0;
            double boosterY = activeEvent.Boosters_Y ?? 1.0;

            return ((int)Math.Round(baseMoveX * boosterX), (int)Math.Round(baseMoveY * boosterY));
        }

       private async Task NotifyAdmin(int gameId, params string[] methods)
        {
            foreach (var method in methods)
            {
                await _hubContext.Clients.Group($"game-{gameId}").SendAsync(method);
            }
        }

        private async Task NotifyTeam(int gameId, int teamId, params string[] methods)
        {
            foreach (var method in methods)
            {
                await _hubContext.Clients.Group($"game-{gameId}-team-{teamId}").SendAsync(method);
            }
        }
    }

    public enum CardType
    {
        Unknown,
        Decision,
        Hardware,
        Software
    }
}