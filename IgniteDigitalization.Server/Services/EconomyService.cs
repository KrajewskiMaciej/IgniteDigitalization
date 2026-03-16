using backend.Data;
using backend.Dtos;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public interface IEconomyService
    {
        /// <summary>Pobierz zasady ekonomii dla danego Szkolenia. Tworzy domyślne jeśli nie istnieją.</summary>
        Task<DeckEconomySettingsDto> GetEconomySettingsAsync(int deckId);

        /// <summary>Zaktualizuj zasady ekonomii dla danego Szkolenia.</summary>
        Task<DeckEconomySettingsDto> UpdateEconomySettingsAsync(int deckId, UpdateDeckEconomySettingsDto dto);

        /// <summary>
        /// Oblicza budżet Mapy 2 dla drużyny na podstawie liczby zagranych kart w fazie PRE.
        /// Formuła: Map2_Base_Budget + (cardsPlayedPhase1 / Map2_Prep_Cards_Total_Count) * Map2_Prep_Bonus_Max_Bits
        /// </summary>
        Task<double> CalculateMap2BudgetAsync(int gameId, int teamId);

        /// <summary>
        /// Oblicza mnożnik przygotowania dla drużyny.
        /// Formuła: MIN(PrepMultiplier_Max; 1 + cardsPlayedPhase1 / Map2_Prep_Cards_Total_Count)
        /// </summary>
        Task<double> GetPreparationMultiplierAsync(int gameId, int teamId);

        /// <summary>
        /// Stosuje mnożnik przygotowania do wag kart (GameProcesses) przy przejściu do Mapy 2.
        /// Mnoży zarówno aktualne pozycje jak i ustawia bazowy mnożnik w GameProcess.
        /// </summary>
        Task ApplyPreparationMultiplierAsync(int gameId, int teamId);

        /// <summary>Tworzy domyślne ustawienia ekonomii dla Szkolenia (używane przy imporcie).</summary>
        Task<DeckEconomySettings> CreateDefaultEconomySettingsAsync(int deckId);

        /// <summary>Podgląd ekonomii dla aktywnej gry (wszystkie drużyny)</summary>
        Task<List<DTOs.TeamEconomyPreviewDto>> GetGameEconomyPreviewAsync(int gameId);
    }

    public class EconomyService : IEconomyService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EconomyService> _logger;

        public EconomyService(AppDbContext context, ILogger<EconomyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DeckEconomySettingsDto> GetEconomySettingsAsync(int deckId)
        {
            var settings = await _context.DeckEconomySettings
                .AsNoTracking()
                .FirstOrDefaultAsync(es => es.Decks_Id == deckId);

            if (settings == null)
            {
                _logger.LogInformation("[EconomyService] Brak ustawień ekonomii dla Szkolenia {DeckId}, tworzę domyślne.", deckId);
                settings = await CreateDefaultEconomySettingsAsync(deckId);
            }

            return ToDto(settings);
        }

        public async Task<DeckEconomySettingsDto> UpdateEconomySettingsAsync(int deckId, UpdateDeckEconomySettingsDto dto)
        {
            var settings = await _context.DeckEconomySettings
                .FirstOrDefaultAsync(es => es.Decks_Id == deckId);

            if (settings == null)
            {
                settings = new DeckEconomySettings { Decks_Id = deckId };
                _context.DeckEconomySettings.Add(settings);
            }

            settings.Map1_Starting_Budget = dto.Map1_Starting_Budget;
            settings.Map1_Mandatory_Cards_Cost = dto.Map1_Mandatory_Cards_Cost;
            settings.Map1_Target_Cards_Min = dto.Map1_Target_Cards_Min;
            settings.Map1_Target_Cards_Max = dto.Map1_Target_Cards_Max;
            settings.Map2_Base_Budget = dto.Map2_Base_Budget;
            settings.Map2_Prep_Bonus_Max_Bits = dto.Map2_Prep_Bonus_Max_Bits;
            settings.Map2_Prep_Cards_Total_Count = dto.Map2_Prep_Cards_Total_Count;
            settings.Map2_Target_Cards_Min = dto.Map2_Target_Cards_Min;
            settings.Map2_Target_Cards_Max = dto.Map2_Target_Cards_Max;
            settings.PrepMultiplier_Max = dto.PrepMultiplier_Max;

            await _context.SaveChangesAsync();
            _logger.LogInformation("[EconomyService] Zaktualizowano zasady ekonomii dla Szkolenia {DeckId}.", deckId);

            return ToDto(settings);
        }

        public async Task<double> CalculateMap2BudgetAsync(int gameId, int teamId)
        {
            var (settings, cardsPlayed) = await GetSettingsAndCardsPlayedAsync(gameId, teamId);
            if (settings == null) return 32; // fallback domyślny

            var bonus = settings.Map2_Prep_Cards_Total_Count > 0
                ? (cardsPlayed / (double)settings.Map2_Prep_Cards_Total_Count) * settings.Map2_Prep_Bonus_Max_Bits
                : 0;

            var budget = settings.Map2_Base_Budget + bonus;
            var budgetCeiled = (double)Math.Ceiling(budget);
            _logger.LogInformation("[EconomyService] Budżet Map2 dla drużyny {TeamId}: {Budget} (zaokrąglony: {Ceiled}, kart PRE: {Cards}/{Total}, premia: {Bonus})",
                teamId, budget, budgetCeiled, cardsPlayed, settings.Map2_Prep_Cards_Total_Count, bonus);

            return budgetCeiled;
        }

        public async Task<double> GetPreparationMultiplierAsync(int gameId, int teamId)
        {
            var (settings, cardsPlayed) = await GetSettingsAndCardsPlayedAsync(gameId, teamId);
            if (settings == null) return 1.0;

            var multiplier = settings.Map2_Prep_Cards_Total_Count > 0
                ? Math.Min(settings.PrepMultiplier_Max, 1.0 + cardsPlayed / (double)settings.Map2_Prep_Cards_Total_Count)
                : 1.0;

            _logger.LogInformation("[EconomyService] Mnożnik przygotowania dla drużyny {TeamId}: {Multiplier} (kart PRE: {Cards}/{Total})",
                teamId, multiplier, cardsPlayed, settings.Map2_Prep_Cards_Total_Count);

            return multiplier;
        }

        public async Task ApplyPreparationMultiplierAsync(int gameId, int teamId)
        {
            var multiplier = await GetPreparationMultiplierAsync(gameId, teamId);

            // Pobierz GameProcessy dla tej drużyny w tej grze
            var gameProcesses = await _context.GameProcesses
                .Where(gp => gp.Games_Id == gameId && gp.Teams_Id == teamId)
                .ToListAsync();

            foreach (var gp in gameProcesses)
            {
                // Zapisz mnożnik w GameProcess (do użycia przez SetGameProcessPosAsync)
                gp.Games_Processes_Weights = (int)Math.Round(multiplier * 100);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("[EconomyService] Zastosowano mnożnik przygotowania {Multiplier} dla drużyny {TeamId} w grze {GameId}.",
                multiplier, teamId, gameId);
        }

        public async Task<DeckEconomySettings> CreateDefaultEconomySettingsAsync(int deckId)
        {
            // Sprawdź czy przypadkiem nie istnieje już (race condition)
            var existing = await _context.DeckEconomySettings.FirstOrDefaultAsync(es => es.Decks_Id == deckId);
            if (existing != null) return existing;

            var defaultSettings = new DeckEconomySettings
            {
                Decks_Id = deckId,
                Map1_Starting_Budget = 40,
                Map1_Mandatory_Cards_Cost = 9,
                Map1_Target_Cards_Min = 14,
                Map1_Target_Cards_Max = 16,
                Map2_Base_Budget = 32,
                Map2_Prep_Bonus_Max_Bits = 12,
                Map2_Prep_Cards_Total_Count = 22,
                Map2_Target_Cards_Min = 11,
                Map2_Target_Cards_Max = 13,
                PrepMultiplier_Max = 2.0
            };

            _context.DeckEconomySettings.Add(defaultSettings);
            await _context.SaveChangesAsync();
            return defaultSettings;
        }

        public async Task<List<DTOs.TeamEconomyPreviewDto>> GetGameEconomyPreviewAsync(int gameId)
        {
            var game = await _context.Games
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Games_Id == gameId);

            if (game == null) return new List<DTOs.TeamEconomyPreviewDto>();

            var settings = await _context.DeckEconomySettings
                .AsNoTracking()
                .FirstOrDefaultAsync(es => es.Decks_Id == game.Decks_Id);

            var teams = await _context.Teams
                .AsNoTracking()
                .Where(t => t.Games_Id == gameId)
                .ToListAsync();

            var result = new List<DTOs.TeamEconomyPreviewDto>();

            foreach (var team in teams)
            {
                var (_, cardsPlayed) = await GetSettingsAndCardsPlayedAsync(gameId, team.Teams_Id);
                double multiplier = 1.0;
                double map2Budget = settings?.Map2_Base_Budget ?? 32;

                if (settings != null)
                {
                    multiplier = settings.Map2_Prep_Cards_Total_Count > 0
                        ? Math.Min(settings.PrepMultiplier_Max, 1.0 + cardsPlayed / (double)settings.Map2_Prep_Cards_Total_Count)
                        : 1.0;

                    var bonus = settings.Map2_Prep_Cards_Total_Count > 0
                        ? (cardsPlayed / (double)settings.Map2_Prep_Cards_Total_Count) * settings.Map2_Prep_Bonus_Max_Bits
                        : 0;
                    map2Budget = settings.Map2_Base_Budget + bonus;
                }

                result.Add(new DTOs.TeamEconomyPreviewDto
                {
                    TeamId = team.Teams_Id,
                    TeamName = team.Teams_Name,
                    CardsPlayedPhase1 = cardsPlayed,
                    PreparationMultiplier = Math.Round(multiplier, 4),
                    ExpectedMap2Budget = Math.Round(map2Budget, 2)
                });
            }

            return result;
        }

        // Pomocnicza: pobiera ustawienia ekonomii i liczbę zagranych kart PRE
        private async Task<(DeckEconomySettings? settings, int cardsPlayed)> GetSettingsAndCardsPlayedAsync(int gameId, int teamId)
        {
            var game = await _context.Games
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Games_Id == gameId);

            if (game == null) return (null, 0);

            var settings = await _context.DeckEconomySettings
                .AsNoTracking()
                .FirstOrDefaultAsync(es => es.Decks_Id == game.Decks_Id);

            // Zlicz karty zagrywane w fazie "Przygotowawcza" (faza 1) przez tę drużynę
            var phase1 = await _context.Phases
                .AsNoTracking()
                .Where(p => p.Decks_Id == game.Decks_Id && p.Phase_Name == "Przygotowawcza")
                .FirstOrDefaultAsync();

            int cardsPlayed = 0;
            if (phase1 != null)
            {
                cardsPlayed = await _context.GameLogs
                    .AsNoTracking()
                    .Where(gl =>
                        gl.Games_Id == gameId &&
                        gl.Teams_Id == teamId &&
                        gl.Is_Approved == true &&
                        gl.Cards != null &&
                        gl.Cards.Phases_Id == phase1.Phases_Id)
                    .CountAsync();
            }

            return (settings, cardsPlayed);
        }

        private static DeckEconomySettingsDto ToDto(DeckEconomySettings s) => new()
        {
            DeckEconomySettings_Id = s.DeckEconomySettings_Id,
            Decks_Id = s.Decks_Id,
            Map1_Starting_Budget = s.Map1_Starting_Budget,
            Map1_Mandatory_Cards_Cost = s.Map1_Mandatory_Cards_Cost,
            Map1_Target_Cards_Min = s.Map1_Target_Cards_Min,
            Map1_Target_Cards_Max = s.Map1_Target_Cards_Max,
            Map2_Base_Budget = s.Map2_Base_Budget,
            Map2_Prep_Bonus_Max_Bits = s.Map2_Prep_Bonus_Max_Bits,
            Map2_Prep_Cards_Total_Count = s.Map2_Prep_Cards_Total_Count,
            Map2_Target_Cards_Min = s.Map2_Target_Cards_Min,
            Map2_Target_Cards_Max = s.Map2_Target_Cards_Max,
            PrepMultiplier_Max = s.PrepMultiplier_Max
        };
    }
}
