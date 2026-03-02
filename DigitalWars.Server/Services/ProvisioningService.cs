using backend.Data;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.Logging;
using System.Drawing;

namespace backend.Services
{
    public interface IProvisioningService
    {
        Task InitializeNewUserAsync(int userId);
        Task<Deck> CreateDeckFromFileForUserAsync(IFormFile file, int userId, string deckName);
        Task ImportEconomySettingsFromFileAsync(int deckId, IFormFile file);
    }

    public class ProvisioningService : IProvisioningService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ProvisioningService> _logger;

        public ProvisioningService(IServiceProvider serviceProvider, ILogger<ProvisioningService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task InitializeNewUserAsync(int userId)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            _logger.LogInformation("[ProvisioningService_InitializeNewUserAsync] Rozpoczynanie inicjalizacji danych dla nowego użytkownika {UserId}", userId);

            var filePath = Path.Combine(AppContext.BaseDirectory, "Initializers", "DigitalWars_UserInitialization.xlsx");
            _logger.LogInformation("[ProvisioningService_InitializeNewUserAsync] Używanie pliku inicjalizacyjnego: {FilePath}", filePath);

            if (!File.Exists(filePath))
            {
                _logger.LogError("[ProvisioningService_InitializeNewUserAsync] Brak pliku inicjalizacyjnego pod ścieżką {FilePath}. Nie można zainicjalizować użytkownika {UserId}.", filePath, userId);
                return;
            }

            try
            {
                using var workbook = new XLWorkbook(filePath);
                _logger.LogInformation("[ProvisioningService_InitializeNewUserAsync] Plik inicjalizacyjny załadowany pomyślnie dla użytkownika {UserId}.", userId);

                var seededBoards = await SeedBoardsForUserFromFileAsync(context, workbook, userId);
                int? teamsDefaultBoardId  = seededBoards.Count > 0 ? seededBoards[0].Boards_Id : null;
                int? rivalsDefaultBoardId = seededBoards.Count > 1 ? seededBoards[1].Boards_Id : null;
                await CreateDeckFromWorkbookAsync(context, workbook, userId, "Talia podstawowa", teamsDefaultBoardId, rivalsDefaultBoardId);

                _logger.LogInformation("[ProvisioningService_InitializeNewUserAsync] Zakończono pomyślnie inicjalizację danych dla użytkownika {UserId}.", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProvisioningService_InitializeNewUserAsync] Wystąpił krytyczny błąd podczas inicjalizacji danych dla użytkownika {UserId}.", userId);
            }
        }

        public async Task<Deck> CreateDeckFromFileForUserAsync(IFormFile file, int userId, string deckName)
        {
            _logger.LogInformation("[ProvisioningService_CreateDeckFromFileForUserAsync] Rozpoczynanie tworzenia talii '{DeckName}' dla użytkownika {UserId} z przesłanego pliku: {FileName}", deckName, userId, file?.FileName);
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("[ProvisioningService_CreateDeckFromFileForUserAsync] Próba utworzenia talii bez przesłania pliku dla użytkownika {UserId}.", userId);
                throw new ArgumentException("Nie przesłano pliku.");
            }

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;
            _logger.LogDebug("[ProvisioningService_CreateDeckFromFileForUserAsync] Plik został skopiowany do strumienia w pamięci dla użytkownika {UserId}", userId);

            using var workbook = new XLWorkbook(stream);
            _logger.LogDebug("[ProvisioningService_CreateDeckFromFileForUserAsync] Stworzono obiekt XLWorkbook ze strumienia dla użytkownika {UserId}", userId);

            // Pobierz plansze użytkownika z bazy – pierwsza = procesów, druga = rywalizacji
            var userBoards = await context.Boards
                .Where(b => b.Users_Id == userId)
                .OrderBy(b => b.Boards_Id)
                .Take(2)
                .ToListAsync();
            int? teamsDefaultBoardId  = userBoards.Count > 0 ? userBoards[0].Boards_Id : null;
            int? rivalsDefaultBoardId = userBoards.Count > 1 ? userBoards[1].Boards_Id : null;

            return await CreateDeckFromWorkbookAsync(context, workbook, userId, deckName, teamsDefaultBoardId, rivalsDefaultBoardId);
        }

        private async Task<Deck> CreateDeckFromWorkbookAsync(AppDbContext context, XLWorkbook workbook, int? userId, string deckName, int? teamsDefaultBoardId = null, int? rivalsDefaultBoardId = null)
        {
            _logger.LogInformation("[ProvisioningService_CreateDeckFromWorkbookAsync] Rozpoczynanie wewnętrznego procesu tworzenia talii '{DeckName}' dla użytkownika {UserId}", deckName, userId);

            ValidateWorkbookStructure(workbook);
            _logger.LogInformation("[ProvisioningService_CreateDeckFromWorkbookAsync] Struktura pliku Excel została pomyślnie zweryfikowana.");

            await using var transaction = await context.Database.BeginTransactionAsync();
            _logger.LogDebug("[ProvisioningService_CreateDeckFromWorkbookAsync] Rozpoczęto transakcję w bazie danych.");
            try
            {
                var newDeck = new Deck
                {
                    Deck_Name = deckName,
                    Users_Id  = userId,
                    Default_Teams_Boards_Id  = teamsDefaultBoardId,
                    Default_Rivals_Boards_Id = rivalsDefaultBoardId
                };
                _logger.LogInformation("[ProvisioningService_CreateDeckFromWorkbookAsync] Domyślna plansza procesów: {TeamsId}, Domyślna plansza rywalizacji: {RivalsId}", teamsDefaultBoardId, rivalsDefaultBoardId);
                context.Decks.Add(newDeck);
                await context.SaveChangesAsync();
                _logger.LogInformation("[ProvisioningService_CreateDeckFromWorkbookAsync] Utworzono nową talię o nazwie '{DeckName}' z ID {DeckId} dla użytkownika {UserId}", deckName, newDeck.Decks_Id, userId);

                var phaseIdMap = await CreatePhaseMapAsync(context, newDeck.Decks_Id);
                var cardIdMap = await LoadAndMapCardsAsync(context, workbook, newDeck.Decks_Id, phaseIdMap);
                var processIdMap = await LoadAndMapEntitiesAsync<Process, int>(context, workbook, "Processes", newDeck.Decks_Id, "Processes_Id", "Processes_Id", phaseIdMap);

                await LoadEntitiesWithoutIdMappingAsync<GameEvent>(context, workbook, "GamesEvents", newDeck.Decks_Id, phaseIdMap);

                LoadRelatedEntities<Decision>(context, workbook, "Decisions", cardIdMap);
                LoadRelatedEntities<Hardware>(context, workbook, "Hardwares", cardIdMap);
                LoadRelatedEntities<Software>(context, workbook, "Softwares", cardIdMap);
                LoadRelatedEntities<Feedback>(context, workbook, "Feedbacks", cardIdMap);
                LoadRelatedEntities<CardWeight>(context, workbook, "CardsWeights", cardIdMap, processIdMap);
                LoadRelatedEntities<CardEnabler>(context, workbook, "CardsEnablers", cardIdMap);

                // Import zasad ekonomii – opcjonalny arkusz EconomySettings
                await ImportEconomySettingsFromWorkbookAsync(context, workbook, newDeck.Decks_Id);

                _logger.LogInformation("[ProvisioningService_CreateDeckFromWorkbookAsync] Wszystkie encje zostały przetworzone. Zapisywanie zmian w bazie danych...");
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogDebug("[ProvisioningService_CreateDeckFromWorkbookAsync] Transakcja została pomyślnie zatwierdzona.");

                _logger.LogInformation("[ProvisioningService_CreateDeckFromWorkbookAsync] Pomyślnie utworzono i zapisano talię '{DeckName}' dla użytkownika {UserId}", deckName, userId);
                return newDeck;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "[ProvisioningService_CreateDeckFromWorkbookAsync] Błąd podczas tworzenia talii '{DeckName}' z pliku Excel. Transakcja została wycofana.", deckName);
                throw;
            }
        }

        private async Task<Dictionary<string, int>> CreatePhaseMapAsync(AppDbContext context, int deckId)
        {
            _logger.LogInformation("[ProvisioningService_CreatePhaseMapAsync] Tworzenie 3 faz dla talii {DeckId}.", deckId);
            var phaseNames = new[] { "Przygotowawcza", "Wejście na rynek", "Rynkowa" };
            var phases = phaseNames.Select(name => new Phase { Decks_Id = deckId, Phase_Name = name }).ToList();
            await context.Phases.AddRangeAsync(phases);
            await context.SaveChangesAsync();

            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < phases.Count; i++)
            {
                map[(i + 1).ToString()] = phases[i].Phases_Id;             // "1", "2", "3"
                map[phases[i].Phase_Name.ToLowerInvariant()] = phases[i].Phases_Id; // "przygotowawcza" etc.
            }
            _logger.LogInformation("[ProvisioningService_CreatePhaseMapAsync] Utworzono mapę faz ({Count} kluczy) dla talii {DeckId}.", map.Count, deckId);
            return map;
        }

        private void ValidateWorkbookStructure(XLWorkbook workbook)
        {
            var requiredSheetsAndColumns = new Dictionary<string, List<string>>
            {
                { "Cards", new List<string> { "Cards_Id" } },
                { "Decisions", new List<string> { "Cards_Id", "Decisions_Short_Desc", "Decisions_Long_Desc", "Decisions_Cost_Bits", "Decisions_Cost_Bits_Weight", "Phase" } },
                { "Feedbacks", new List<string> { "Cards_Id", "Status", "Feedbacks_Long_Description" } },
                { "Processes", new List<string> { "Processes_Id", "Processes_Desc", "Processes_Long_Desc", "Processes_Color", "Processes_Weight" } },
                { "CardsWeights", new List<string> { "Cards_Id", "Processes_Id", "Weights_X", "Weights_Y" } },
                { "CardsEnablers", new List<string> { "Cards_Id", "Enablers_Id", "Cards_Enablers_Description" } }
            };

            foreach (var requiredSheet in requiredSheetsAndColumns)
            {
                if (!workbook.Worksheets.TryGetWorksheet(requiredSheet.Key, out var sheet))
                {
                    throw new InvalidDataException($"Brak wymaganego arkusza o nazwie '{requiredSheet.Key}' w pliku Excel.");
                }

                var headerRow = sheet.Row(1);
                var actualHeaders = headerRow.CellsUsed().Select(c => c.GetString()).ToList();
                var missingHeaders = requiredSheet.Value.Except(actualHeaders).ToList();

                if (missingHeaders.Any())
                {
                    throw new InvalidDataException($"W arkuszu '{requiredSheet.Key}' brakuje wymaganych kolumn: {string.Join(", ", missingHeaders)}.");
                }
            }
        }

        public async Task ImportEconomySettingsFromFileAsync(int deckId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Nie przesłano pliku.");

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            await ImportEconomySettingsFromWorkbookAsync(context, workbook, deckId);
            await context.SaveChangesAsync();

            _logger.LogInformation("[ProvisioningService] Zaimportowano zasady ekonomii dla Szkolenia {DeckId} z pliku {FileName}.", deckId, file.FileName);
        }

        private async Task ImportEconomySettingsFromWorkbookAsync(AppDbContext context, XLWorkbook workbook, int deckId)
        {
            if (!workbook.TryGetWorksheet("EconomySettings", out var sheet))
            {
                _logger.LogInformation("[ProvisioningService] Brak arkusza 'EconomySettings' w pliku. Tworzę domyślne ustawienia ekonomii dla Szkolenia {DeckId}.", deckId);
                // Utwórz domyślne jeśli nie istnieją
                var existingDefault = await context.DeckEconomySettings.FirstOrDefaultAsync(es => es.Decks_Id == deckId);
                if (existingDefault == null)
                {
                    context.DeckEconomySettings.Add(new DeckEconomySettings { Decks_Id = deckId });
                }
                return;
            }

            _logger.LogInformation("[ProvisioningService] Wczytujemy arkusz 'EconomySettings' dla Szkolenia {DeckId}.", deckId);

            var settings = await context.DeckEconomySettings.FirstOrDefaultAsync(es => es.Decks_Id == deckId);
            if (settings == null)
            {
                settings = new DeckEconomySettings { Decks_Id = deckId };
                context.DeckEconomySettings.Add(settings);
            }

            // Parsuj arkusz z układem: kolumna A = nazwa pola, kolumna B = wartość
            var headers = sheet.Row(1).CellsUsed().ToDictionary(c => c.Address.ColumnNumber, c => c.GetString());

            // Obsługujemy dwa formaty:
            // Format 1: Wiersz 1 = nagłówki (Map1_Starting_Budget, ...), Wiersz 2 = wartości
            // Format 2: Kolumna A = nazwa, Kolumna B = wartość (wiersze)

            bool isHorizontal = headers.Values.Any(h =>
                h.Equals("Map1_Starting_Budget", StringComparison.OrdinalIgnoreCase) ||
                h.Equals("Map2_Base_Budget", StringComparison.OrdinalIgnoreCase));

            bool isNaturalLanguage = !isHorizontal && !headers.Values.Any(h =>
                h.Equals("Map1_Starting_Budget", StringComparison.OrdinalIgnoreCase) ||
                h.Equals("Map1_Mandatory_Cards_Cost", StringComparison.OrdinalIgnoreCase)) &&
                sheet.RowsUsed().Take(25).Any(r =>
                {
                    var txt = r.Cell(1).GetString().ToLowerInvariant();
                    return txt.Contains("budżet") || txt.Contains("budzet") || txt.Contains("mapa") ||
                           txt.Contains("ekonomia") || txt.Contains("mnożnik") || txt.Contains("bonus");
                });

            if (isHorizontal)
            {
                // Nagłówki w wierszu 1, wartości w wierszu 2
                var dataRow = sheet.Row(2);
                var colMap = headers.ToDictionary(kvp => kvp.Value, kvp => kvp.Key, StringComparer.OrdinalIgnoreCase);

                if (colMap.TryGetValue("Map1_Starting_Budget", out var col))
                    settings.Map1_Starting_Budget = ParseDouble(dataRow.Cell(col).GetString()) ?? settings.Map1_Starting_Budget;
                if (colMap.TryGetValue("Map1_Mandatory_Cards_Cost", out col))
                    settings.Map1_Mandatory_Cards_Cost = ParseDouble(dataRow.Cell(col).GetString()) ?? settings.Map1_Mandatory_Cards_Cost;
                if (colMap.TryGetValue("Map1_Target_Cards_Min", out col))
                    settings.Map1_Target_Cards_Min = ParseInt(dataRow.Cell(col).GetString()) ?? settings.Map1_Target_Cards_Min;
                if (colMap.TryGetValue("Map1_Target_Cards_Max", out col))
                    settings.Map1_Target_Cards_Max = ParseInt(dataRow.Cell(col).GetString()) ?? settings.Map1_Target_Cards_Max;
                if (colMap.TryGetValue("Map2_Base_Budget", out col))
                    settings.Map2_Base_Budget = ParseDouble(dataRow.Cell(col).GetString()) ?? settings.Map2_Base_Budget;
                if (colMap.TryGetValue("Map2_Prep_Bonus_Max_Bits", out col))
                    settings.Map2_Prep_Bonus_Max_Bits = ParseDouble(dataRow.Cell(col).GetString()) ?? settings.Map2_Prep_Bonus_Max_Bits;
                if (colMap.TryGetValue("Map2_Prep_Cards_Total_Count", out col))
                    settings.Map2_Prep_Cards_Total_Count = ParseInt(dataRow.Cell(col).GetString()) ?? settings.Map2_Prep_Cards_Total_Count;
                if (colMap.TryGetValue("Map2_Target_Cards_Min", out col))
                    settings.Map2_Target_Cards_Min = ParseInt(dataRow.Cell(col).GetString()) ?? settings.Map2_Target_Cards_Min;
                if (colMap.TryGetValue("Map2_Target_Cards_Max", out col))
                    settings.Map2_Target_Cards_Max = ParseInt(dataRow.Cell(col).GetString()) ?? settings.Map2_Target_Cards_Max;
                if (colMap.TryGetValue("PrepMultiplier_Max", out col))
                    settings.PrepMultiplier_Max = ParseDouble(dataRow.Cell(col).GetString()) ?? settings.PrepMultiplier_Max;
            }
            else if (isNaturalLanguage)
            {
                // Format naturalny: dokument polski z etykietami i wartościami z jednostkami
                // np. "Budżet startowy" | "40 bitów", "Cel projektowy Map 1" | "14-16 kart zagranych"
                ParseNaturalLanguageEconomySheet(sheet, settings);
            }
            else
            {
                // Pionowy układ: Kolumna A = nazwa (angielska), Kolumna B = wartość
                foreach (var row in sheet.RowsUsed().Skip(1))
                {
                    var fieldName = row.Cell(1).GetString().Trim();
                    var value = row.Cell(2).GetString().Trim();

                    switch (fieldName)
                    {
                        case "Map1_Starting_Budget": settings.Map1_Starting_Budget = ParseDouble(value) ?? settings.Map1_Starting_Budget; break;
                        case "Map1_Mandatory_Cards_Cost": settings.Map1_Mandatory_Cards_Cost = ParseDouble(value) ?? settings.Map1_Mandatory_Cards_Cost; break;
                        case "Map1_Target_Cards_Min": settings.Map1_Target_Cards_Min = ParseInt(value) ?? settings.Map1_Target_Cards_Min; break;
                        case "Map1_Target_Cards_Max": settings.Map1_Target_Cards_Max = ParseInt(value) ?? settings.Map1_Target_Cards_Max; break;
                        case "Map2_Base_Budget": settings.Map2_Base_Budget = ParseDouble(value) ?? settings.Map2_Base_Budget; break;
                        case "Map2_Prep_Bonus_Max_Bits": settings.Map2_Prep_Bonus_Max_Bits = ParseDouble(value) ?? settings.Map2_Prep_Bonus_Max_Bits; break;
                        case "Map2_Prep_Cards_Total_Count": settings.Map2_Prep_Cards_Total_Count = ParseInt(value) ?? settings.Map2_Prep_Cards_Total_Count; break;
                        case "Map2_Target_Cards_Min": settings.Map2_Target_Cards_Min = ParseInt(value) ?? settings.Map2_Target_Cards_Min; break;
                        case "Map2_Target_Cards_Max": settings.Map2_Target_Cards_Max = ParseInt(value) ?? settings.Map2_Target_Cards_Max; break;
                        case "PrepMultiplier_Max": settings.PrepMultiplier_Max = ParseDouble(value) ?? settings.PrepMultiplier_Max; break;
                    }
                }
            }

            _logger.LogInformation("[ProvisioningService] Zasady ekonomii dla Szkolenia {DeckId} wczytane pomyślnie.", deckId);
        }

        // -------------------------------------------------------------------------
        // Parsowanie formatu naturalnego (dokument polski)
        // np. "Budżet startowy" | "40 bitów"
        //     "Cel projektowy Map 1" | "14-16 kart zagranych"
        //     "Bonus przygotowania" | "(kart_PRE / 22) × 12 bitów"
        //     "Formuła" | "MIN(2,0; 1 + kart_PRE_zagranych / 22)"
        // -------------------------------------------------------------------------
        private static void ParseNaturalLanguageEconomySheet(IXLWorksheet sheet, DeckEconomySettings settings)
        {
            foreach (var row in sheet.RowsUsed())
            {
                // Zbieramy tekst ze wszystkich komórek wiersza jako jeden string
                var cells = row.CellsUsed().Select(c => c.GetString().Trim()).ToList();
                if (cells.Count == 0) continue;

                // Etykieta to kolumna A, wartość to kolumna B (lub kolejne komórki)
                var label = cells[0].ToLowerInvariant();
                var valueCells = cells.Skip(1).ToList();
                var valueText = string.Join(" ", valueCells);

                // Budżet startowy → Map1_Starting_Budget
                if (label.Contains("budżet startowy") || label.Contains("budzet startowy"))
                {
                    var n = ExtractFirstNumber(valueText);
                    if (n.HasValue) settings.Map1_Starting_Budget = n.Value;
                }
                // Karty obowiązkowe → Map1_Mandatory_Cards_Cost (szukamy "koszt: X" lub samodzielnej liczby)
                else if (label.Contains("karty obowiązkowe") || label.Contains("mandatory"))
                {
                    // Szukamy wzorca "koszt: 9" lub "9 bitów" w którymkolwiek tekście
                    var allText = string.Join(" ", cells).ToLowerInvariant();
                    var m = System.Text.RegularExpressions.Regex.Match(allText, @"koszt[:\s]+(\d+)");
                    if (m.Success)
                        settings.Map1_Mandatory_Cards_Cost = double.Parse(m.Groups[1].Value);
                    else
                    {
                        var n = ExtractFirstNumber(valueText);
                        if (n.HasValue) settings.Map1_Mandatory_Cards_Cost = n.Value;
                    }
                }
                // Cel projektowy Map 1 → Min/Max
                else if ((label.Contains("cel projektowy") || label.Contains("cel projektu")) &&
                         (label.Contains("map 1") || label.Contains("mapa 1")))
                {
                    var (min, max) = ExtractRange(valueText);
                    if (min.HasValue) settings.Map1_Target_Cards_Min = (int)min.Value;
                    if (max.HasValue) settings.Map1_Target_Cards_Max = (int)max.Value;
                }
                // Budżet bazowy → Map2_Base_Budget
                else if (label.Contains("budżet bazowy") || label.Contains("budzet bazowy"))
                {
                    var n = ExtractFirstNumber(valueText);
                    if (n.HasValue) settings.Map2_Base_Budget = n.Value;
                }
                // Bonus przygotowania → "(kart_PRE / 22) × 12 bitów"
                // 22 = Map2_Prep_Cards_Total_Count, 12 = Map2_Prep_Bonus_Max_Bits
                else if (label.Contains("bonus przygotowania") || label.Contains("bonus_pre") || label.Contains("bonus pre"))
                {
                    var numbers = ExtractAllNumbers(valueText);
                    // Wzorzec: pierwsza liczba po "/" to total count (22), po "×" to bonus (12)
                    var mSlash = System.Text.RegularExpressions.Regex.Match(valueText, @"/\s*(\d+)");
                    var mMul = System.Text.RegularExpressions.Regex.Match(valueText, @"[×x\*]\s*([\d,\.]+)");
                    if (mSlash.Success)
                        settings.Map2_Prep_Cards_Total_Count = int.Parse(mSlash.Groups[1].Value);
                    if (mMul.Success)
                        settings.Map2_Prep_Bonus_Max_Bits = ParseDouble(mMul.Groups[1].Value) ?? settings.Map2_Prep_Bonus_Max_Bits;
                    else if (numbers.Count >= 2)
                    {
                        settings.Map2_Prep_Cards_Total_Count = (int)numbers[0];
                        settings.Map2_Prep_Bonus_Max_Bits = numbers[1];
                    }
                }
                // Cel projektowy Map 2 → Min/Max
                else if ((label.Contains("cel projektowy") || label.Contains("cel projektu")) &&
                         (label.Contains("map 2") || label.Contains("mapa 2")))
                {
                    var (min, max) = ExtractRange(valueText);
                    if (min.HasValue) settings.Map2_Target_Cards_Min = (int)min.Value;
                    if (max.HasValue) settings.Map2_Target_Cards_Max = (int)max.Value;
                }
                // Formuła mnożnika → "MIN(2,0; 1 + kart_PRE_zagranych / 22)"
                else if (label.Contains("formuła") || label.Contains("formula") ||
                         (label.Contains("mnożnik") && !label.Contains("reguła")))
                {
                    // Szukamy MIN(X, ...) lub MAX = X
                    var mMin = System.Text.RegularExpressions.Regex.Match(valueText, @"MIN\s*\(\s*([\d,\.]+)");
                    if (mMin.Success)
                        settings.PrepMultiplier_Max = ParseDouble(mMin.Groups[1].Value) ?? settings.PrepMultiplier_Max;
                    else
                    {
                        var n = ExtractFirstNumber(valueText);
                        if (n.HasValue) settings.PrepMultiplier_Max = n.Value;
                    }
                }
            }
        }

        private static double? ExtractFirstNumber(string text)
        {
            var m = System.Text.RegularExpressions.Regex.Match(text, @"[\d]+(?:[,\.][\d]+)?");
            if (!m.Success) return null;
            return ParseDouble(m.Value);
        }

        private static List<double> ExtractAllNumbers(string text)
        {
            var matches = System.Text.RegularExpressions.Regex.Matches(text, @"[\d]+(?:[,\.][\d]+)?");
            return matches.Select(m => ParseDouble(m.Value) ?? 0.0).ToList();
        }

        /// <summary>Wyciąga zakres "X-Y" lub "X–Y" z tekstu, np. "14-16 kart zagranych".</summary>
        private static (double? min, double? max) ExtractRange(string text)
        {
            var m = System.Text.RegularExpressions.Regex.Match(text, @"([\d]+(?:[,\.][\d]+)?)\s*[-–]\s*([\d]+(?:[,\.][\d]+)?)");
            if (m.Success)
                return (ParseDouble(m.Groups[1].Value), ParseDouble(m.Groups[2].Value));
            var single = ExtractFirstNumber(text);
            return (single, null);
        }

        private static double? ParseDouble(string val)
        {
            val = val.Replace(',', '.');
            return double.TryParse(val, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var r) ? r : null;
        }

        private static int? ParseInt(string val)
        {
            val = val.Replace(',', '.');
            return int.TryParse(val, out var r) ? r : null;
        }

        private async Task<List<Board>> SeedBoardsForUserFromFileAsync(AppDbContext context, XLWorkbook workbook, int userId)
        {
            _logger.LogInformation("[ProvisioningService_SeedBoardsForUserFromFileAsync] Rozpoczynanie seedowania plansz dla użytkownika {UserId}", userId);
            if (await context.Boards.AnyAsync(b => b.Users_Id == userId))
            {
                _logger.LogInformation("[ProvisioningService_SeedBoardsForUserFromFileAsync] Plansze dla użytkownika {UserId} już istnieją. Pomijanie seedowania.", userId);
                // Zwróć istniejące plansze w kolejności (potrzebne do przypisania domyślnych plansz do talii)
                return await context.Boards
                    .Where(b => b.Users_Id == userId)
                    .OrderBy(b => b.Boards_Id)
                    .ToListAsync();
            }

            if (!workbook.TryGetWorksheet("Boards", out var sheet))
            {
                _logger.LogWarning("[ProvisioningService_SeedBoardsForUserFromFileAsync] Arkusz 'Boards' nie został znaleziony w pliku. Nie można zainicjalizować plansz.");
                return new List<Board>();
            }

            var boardsToAdd = new List<Board>();
            foreach (var row in sheet.RowsUsed().Skip(1))
            {
                if (row.IsEmpty())
                {
                    continue;
                }
                boardsToAdd.Add(new Board
                {
                    Users_Id = userId,
                    Name = row.Cell(1).GetString(),
                    Labels_Up = row.Cell(2).GetString(),
                    Labels_Right = row.Cell(3).GetString(),
                    Description_Down = row.Cell(4).GetString(),
                    Description_Left = row.Cell(5).GetString(),
                    Rows = row.Cell(6).GetValue<int>(),
                    Cols = row.Cell(7).GetValue<int>(),
                    Border_Color = row.Cell(8).GetString(),
                    Cell_Color = row.Cell(9).GetString(),
                    Borders_Colors = row.Cell(10).GetString(),
                    Cells_Descriptions = row.Cell(11).GetString()
                });
            }

            if (boardsToAdd.Any())
            {
                _logger.LogInformation("[ProvisioningService_SeedBoardsForUserFromFileAsync] Znaleziono {Count} plansz do dodania dla użytkownika {UserId}.", boardsToAdd.Count, userId);
                context.Boards.AddRange(boardsToAdd);
                await context.SaveChangesAsync();
                _logger.LogInformation("[ProvisioningService_SeedBoardsForUserFromFileAsync] Pomyślnie dodano {Count} plansz dla użytkownika {UserId}. Pierwsza plansza ID={FirstId}, Druga plansza ID={SecondId}.",
                    boardsToAdd.Count, userId,
                    boardsToAdd.Count > 0 ? boardsToAdd[0].Boards_Id : (int?)null,
                    boardsToAdd.Count > 1 ? boardsToAdd[1].Boards_Id : (int?)null);
                return boardsToAdd;
            }
            else
            {
                _logger.LogWarning("[ProvisioningService_SeedBoardsForUserFromFileAsync] Arkusz 'Boards' został znaleziony, ale jest pusty. Nie dodano żadnych plansz dla użytkownika {UserId}.", userId);
                return new List<Board>();
            }
        }

        private async Task LoadEntitiesWithoutIdMappingAsync<TEntity>(AppDbContext context, XLWorkbook workbook, string sheetName, int deckId, Dictionary<string, int>? phaseIdMap = null) where TEntity : class, new()
        {
            _logger.LogInformation("[ProvisioningService_LoadEntitiesWithoutIdMappingAsync] Ładowanie encji typu '{EntityType}' z arkusza '{SheetName}' bez mapowania ID.", typeof(TEntity).Name, sheetName);
            if (!workbook.TryGetWorksheet(sheetName, out var sheet))
            {
                _logger.LogWarning("[ProvisioningService_LoadEntitiesWithoutIdMappingAsync] Arkusz '{SheetName}' nie został znaleziony, pomijanie.", sheetName);
                return;
            }

            var properties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);
            var headers = sheet.Row(1).CellsUsed().ToDictionary(cell => cell.Address.ColumnNumber, cell => cell.GetString());
            var entitiesToAdd = new List<TEntity>();

            _logger.LogDebug("[ProvisioningService_LoadEntitiesWithoutIdMappingAsync] Znaleziono {HeaderCount} nagłówków w arkuszu '{SheetName}'.", headers.Count, sheetName);

            foreach (var row in sheet.RowsUsed().Skip(1))
            {
                if (row.IsEmpty())
                {
                    continue;
                }

                var entity = new TEntity();
                properties.GetValueOrDefault("Decks_Id")?.SetValue(entity, deckId);

                foreach (var cell in row.CellsUsed())
                {
                    if (!headers.TryGetValue(cell.Address.ColumnNumber, out var propName)) continue;

                    string resolvedPropName = (string.Equals(propName, "Phase", StringComparison.OrdinalIgnoreCase) ||
                                               string.Equals(propName, "Phase_Id", StringComparison.OrdinalIgnoreCase))
                        ? "Phases_Id" : propName;

                    if (!properties.TryGetValue(resolvedPropName, out var prop)) continue;

                    if (string.Equals(resolvedPropName, "Phases_Id", StringComparison.OrdinalIgnoreCase) && phaseIdMap != null)
                    {
                        var phaseValue = cell.GetString().Trim().ToLowerInvariant();
                        if (phaseIdMap.TryGetValue(phaseValue, out int phaseDbId))
                            prop.SetValue(entity, phaseDbId);
                        else if (!string.IsNullOrWhiteSpace(phaseValue))
                            _logger.LogWarning("[ProvisioningService_LoadEntitiesWithoutIdMappingAsync] Nieznana wartość fazy '{PhaseValue}' w arkuszu '{SheetName}' wiersz {RowNumber}.", phaseValue, sheetName, row.RowNumber());
                        continue;
                    }

                    var valueToSet = ConvertValue(cell.GetString(), prop.PropertyType, cell.DataType);
                    var isNullable = Nullable.GetUnderlyingType(prop.PropertyType) != null || prop.PropertyType == typeof(string);
                    if (valueToSet == null && !isNullable)
                    {
                        prop.SetValue(entity, GetDefaultValue(prop.PropertyType));
                    }
                    else
                    {
                        prop.SetValue(entity, valueToSet);
                    }
                }
                entitiesToAdd.Add(entity);
            }

            if (entitiesToAdd.Any())
            {
                await context.Set<TEntity>().AddRangeAsync(entitiesToAdd);
                _logger.LogInformation("[ProvisioningService_LoadEntitiesWithoutIdMappingAsync] Przygotowano {Count} encji typu '{EntityType}' z arkusza '{SheetName}' do dodania.", entitiesToAdd.Count, typeof(TEntity).Name, sheetName);
            }
            else
            {
                _logger.LogWarning("[ProvisioningService_LoadEntitiesWithoutIdMappingAsync] Nie znaleziono danych do załadowania w arkuszu '{SheetName}'.", sheetName);
            }
        }

        private async Task<Dictionary<int, int>> LoadAndMapCardsAsync(AppDbContext context, XLWorkbook workbook, int deckId, Dictionary<string, int>? phaseIdMap = null)
        {
            _logger.LogInformation("[ProvisioningService_LoadAndMapCardsAsync] Rozpoczynanie ładowania i mapowania kart dla talii {DeckId}.", deckId);
            if (!workbook.TryGetWorksheet("Cards", out var sheet))
            {
                _logger.LogError("[ProvisioningService_LoadAndMapCardsAsync] Nie znaleziono wymaganego arkusza 'Cards' w pliku Excel. Dalsze przetwarzanie może być niemożliwe.");
                throw new InvalidDataException("Arkusz 'Cards' jest wymagany, ale nie został znaleziony.");
            }

            var headers = sheet.Row(1).CellsUsed().ToDictionary(cell => cell.Address.ColumnNumber, cell => cell.GetString());
            int? phaseColumnNumber = null;
            if (phaseIdMap != null)
            {
                var phaseHeader = headers.FirstOrDefault(h =>
                    string.Equals(h.Value, "Phase", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(h.Value, "Phase_Id", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(h.Value, "Phases_Id", StringComparison.OrdinalIgnoreCase));
                if (phaseHeader.Key != 0) phaseColumnNumber = phaseHeader.Key;
            }

            var cardsToAdd = new List<Card>();
            foreach (var row in sheet.RowsUsed().Skip(1))
            {
                if (row.IsEmpty())
                {
                    continue;
                }

                if (row.Cell(1).TryGetValue(out int cardIdFromExcel))
                {
                    var card = new Card { Decks_Id = deckId, Card_Id = cardIdFromExcel };

                    if (phaseColumnNumber.HasValue && phaseIdMap != null)
                    {
                        var phaseValue = row.Cell(phaseColumnNumber.Value).GetString().Trim().ToLowerInvariant();
                        if (phaseIdMap.TryGetValue(phaseValue, out int phaseDbId))
                            card.Phases_Id = phaseDbId;
                        else if (!string.IsNullOrWhiteSpace(phaseValue))
                            _logger.LogWarning("[ProvisioningService_LoadAndMapCardsAsync] Nieznana wartość fazy '{PhaseValue}' w wierszu {RowNumber} w arkuszu 'Cards'.", phaseValue, row.RowNumber());
                    }

                    cardsToAdd.Add(card);
                }
            }

            if (!cardsToAdd.Any())
            {
                _logger.LogWarning("[ProvisioningService_LoadAndMapCardsAsync] Arkusz 'Cards' jest pusty lub nie zawiera prawidłowych ID kart dla talii {DeckId}.", deckId);
                return new Dictionary<int, int>();
            }

            _logger.LogInformation("[ProvisioningService_LoadAndMapCardsAsync] Znaleziono {CardCount} kart w arkuszu 'Cards' do dodania dla talii {DeckId}.", cardsToAdd.Count, deckId);
            context.Cards.AddRange(cardsToAdd);
            await context.SaveChangesAsync();
            _logger.LogDebug("[ProvisioningService_LoadAndMapCardsAsync] Karty zostały zapisane w bazie danych dla talii {DeckId}.", deckId);

            var idMap = cardsToAdd.ToDictionary(
                card => card.Card_Id,
                card => card.Cards_Id
            );

            _logger.LogInformation("[ProvisioningService_LoadAndMapCardsAsync] Pomyślnie zmapowano {MapCount} identyfikatorów kart (Excel ID -> DB ID) dla talii {DeckId}.", idMap.Count, deckId);
            return idMap;
        }

        private async Task<Dictionary<TKey, TKey>> LoadAndMapEntitiesAsync<TEntity, TKey>(AppDbContext context, XLWorkbook workbook, string sheetName, int deckId, string excelIdColumn, string dbIdColumn, Dictionary<string, int>? phaseIdMap = null) where TEntity : class, new() where TKey : notnull
        {
            _logger.LogInformation("[ProvisioningService_LoadAndMapEntitiesAsync] Rozpoczynanie ładowania i mapowania encji '{EntityType}' z arkusza '{SheetName}'.", typeof(TEntity).Name, sheetName);
            if (!workbook.TryGetWorksheet(sheetName, out var sheet))
            {
                _logger.LogWarning("[ProvisioningService_LoadAndMapEntitiesAsync] Arkusz '{SheetName}' nie został znaleziony, pomijanie.", sheetName);
                return new Dictionary<TKey, TKey>();
            }

            var properties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);
            var headers = sheet.Row(1).CellsUsed().ToDictionary(cell => cell.Address.ColumnNumber, cell => cell.GetString());
            var entitiesWithExcelId = new List<(TEntity entity, TKey excelId)>();

            foreach (var row in sheet.RowsUsed().Skip(1))
            {
                if (row.IsEmpty())
                {
                    continue;
                }

                var entity = new TEntity();
                properties.GetValueOrDefault("Decks_Id")?.SetValue(entity, deckId);
                TKey? excelIdValue = default;

                var excelIdCell = row.CellsUsed().FirstOrDefault(c => headers.ContainsKey(c.Address.ColumnNumber) && headers[c.Address.ColumnNumber].Equals(excelIdColumn, StringComparison.OrdinalIgnoreCase));
                if (excelIdCell != null)
                {
                    var convertedId = ConvertValue(excelIdCell.GetString(), properties[excelIdColumn].PropertyType, excelIdCell.DataType);
                    if (convertedId is TKey id)
                    {
                        excelIdValue = id;
                    }
                }

                if (excelIdValue == null || excelIdValue.Equals(default(TKey)))
                {
                    _logger.LogError("[ProvisioningService_LoadAndMapEntitiesAsync] Brak lub nieprawidłowa wartość w kolumnie ID '{ExcelIdColumn}' w arkuszu '{SheetName}' w wierszu {RowNumber}.", excelIdColumn, sheetName, row.RowNumber());
                    throw new InvalidDataException($"Brak lub nieprawidłowa wartość w kolumnie ID '{excelIdColumn}' w arkuszu '{sheetName}' w wierszu {row.RowNumber()}.");
                }

                foreach (var cell in row.CellsUsed())
                {
                    if (headers.TryGetValue(cell.Address.ColumnNumber, out var propName) && properties.TryGetValue(propName, out var prop))
                    {
                        if (prop.Name.Equals(dbIdColumn, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        object? valueToSet;
                        if (typeof(TEntity) == typeof(Process) && prop.Name.Equals("Processes_Color", StringComparison.OrdinalIgnoreCase))
                        {
                            string colorValue = cell.GetString();
                            valueToSet = ConvertColorToHex(colorValue, sheetName, row.RowNumber());
                        }
                        else
                        {
                            valueToSet = ConvertValue(cell.GetString(), prop.PropertyType, cell.DataType);
                        }

                        var isNullable = Nullable.GetUnderlyingType(prop.PropertyType) != null || prop.PropertyType == typeof(string);
                        if (valueToSet == null && !isNullable)
                        {
                            prop.SetValue(entity, GetDefaultValue(prop.PropertyType));
                        }
                        else
                        {
                            prop.SetValue(entity, valueToSet);
                        }
                    }
                }
                entitiesWithExcelId.Add((entity, excelIdValue));
            }

            if (entitiesWithExcelId.Any())
            {
                _logger.LogInformation("[ProvisioningService_LoadAndMapEntitiesAsync] Znaleziono {Count} encji '{EntityType}' w arkuszu '{SheetName}' do dodania.", entitiesWithExcelId.Count, typeof(TEntity).Name, sheetName);
                await context.Set<TEntity>().AddRangeAsync(entitiesWithExcelId.Select(t => t.entity));
                await context.SaveChangesAsync();
                _logger.LogDebug("[ProvisioningService_LoadAndMapEntitiesAsync] Encje '{EntityType}' zostały zapisane w bazie danych.", typeof(TEntity).Name);

                var dbIdProperty = properties[dbIdColumn];
                var idMap = entitiesWithExcelId.ToDictionary(
                    t => t.excelId,
                    t => (TKey)dbIdProperty.GetValue(t.entity)!
                );
                _logger.LogInformation("[ProvisioningService_LoadAndMapEntitiesAsync] Pomyślnie zmapowano {MapCount} identyfikatorów dla '{EntityType}'.", idMap.Count, typeof(TEntity).Name);
                return idMap;
            }

            _logger.LogWarning("[ProvisioningService_LoadAndMapEntitiesAsync] Nie znaleziono żadnych encji '{EntityType}' do załadowania w arkuszu '{SheetName}'.", typeof(TEntity).Name, sheetName);
            return new Dictionary<TKey, TKey>();
        }

        private void LoadRelatedEntities<TEntity>(AppDbContext context, XLWorkbook workbook, string sheetName, Dictionary<int, int> cardIdMap, Dictionary<int, int>? processIdMap = null) where TEntity : class, new()
        {
            _logger.LogInformation("[ProvisioningService_LoadRelatedEntities] Ładowanie powiązanych encji '{EntityType}' z arkusza '{SheetName}'.", typeof(TEntity).Name, sheetName);
            if (!workbook.TryGetWorksheet(sheetName, out var sheet))
            {
                _logger.LogWarning("[ProvisioningService_LoadRelatedEntities] Arkusz '{SheetName}' nie został znaleziony. Pomijanie ładowania encji '{EntityType}'.", sheetName, typeof(TEntity).Name);
                return;
            }

            var properties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);
            var cardForeignKeyPropertyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Cards_Id", "Enablers_Id" };
            var headers = sheet.Row(1).CellsUsed().ToDictionary(cell => cell.Address.ColumnNumber, cell => cell.GetString());
            var primaryKeyColumnName = sheetName.TrimEnd('s') + "_Id";
            var primaryKeyColumnNamePlural = sheetName + "_Id";
            var entitiesToAdd = new List<TEntity>();

            // Znajdź numer kolumny dla 'Cards_Id', aby móc sprawdzać ją na początku
            var cardIdHeader = headers.FirstOrDefault(h =>
                string.Equals(h.Value, "Cards_Id", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(h.Value, "Card_Id", StringComparison.OrdinalIgnoreCase));

            // Jeśli arkusz nie ma kolumny Cards_Id (np. GamesEvents), ten warunek nie będzie sprawdzany
            bool requiresCardId = properties.ContainsKey("Cards_Id");
            int cardIdColumnNumber = requiresCardId ? cardIdHeader.Key : -1;

            if (requiresCardId && cardIdColumnNumber == -1)
            {
                // To zabezpieczenie, chociaż ValidateWorkbookStructure powinno to wyłapać
                throw new InvalidDataException($"W arkuszu '{sheetName}' brakuje wymaganej kolumny 'Cards_Id' lub 'Card_Id'.");
            }

            foreach (var row in sheet.RowsUsed().Skip(1))
            {
                if (row.IsEmpty())
                {
                    continue;
                }

                // Jeśli arkusz wymaga 'Cards_Id', sprawdź, czy komórka w tej kolumnie jest pusta lub nie jest liczbą.
                // Jeśli tak, zignoruj cały wiersz jako nieprawidłowy.
                if (requiresCardId)
                {
                    var cardIdCell = row.Cell(cardIdColumnNumber);
                    if (cardIdCell.IsEmpty() || !cardIdCell.TryGetValue(out int _))
                    {
                        _logger.LogWarning("[ProvisioningService_LoadRelatedEntities] Pomijanie wiersza {RowNumber} w arkuszu '{SheetName}', ponieważ brakuje w nim prawidłowej wartości w kolumnie 'Cards_Id'.", row.RowNumber(), sheetName);
                        continue;
                    }
                }

                var entity = new TEntity();
                foreach (var cell in row.CellsUsed())
                {
                    if (!headers.TryGetValue(cell.Address.ColumnNumber, out var headerName) || string.IsNullOrWhiteSpace(headerName)) continue;

                    if (string.Equals(headerName, primaryKeyColumnName, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(headerName, primaryKeyColumnNamePlural, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string propertyName = headerName;

                    if (string.Equals(headerName, "Card_Id", StringComparison.OrdinalIgnoreCase) || string.Equals(headerName, "CardId", StringComparison.OrdinalIgnoreCase))
                    {
                        propertyName = "Cards_Id";
                    }
                    else if (string.Equals(headerName, "Enablers_Id", StringComparison.OrdinalIgnoreCase) || string.Equals(headerName, "Enabler_Id", StringComparison.OrdinalIgnoreCase) || string.Equals(headerName, "Enabler_Card_Id", StringComparison.OrdinalIgnoreCase))
                    {
                        propertyName = "Enablers_Id";
                    }
                    else if (string.Equals(headerName, "ProcessId", StringComparison.OrdinalIgnoreCase) || string.Equals(headerName, "Processes_Id", StringComparison.OrdinalIgnoreCase))
                    {
                        propertyName = "Processes_Id";
                    }

                    if (!properties.TryGetValue(propertyName, out var prop)) continue;

                    if (cardForeignKeyPropertyNames.Contains(propertyName))
                    {
                        if (cell.TryGetValue(out int idFromExcel) && cardIdMap.TryGetValue(idFromExcel, out int mappedDbId))
                        {
                            prop.SetValue(entity, mappedDbId);
                        }
                        else if (cell.TryGetValue(out idFromExcel))
                        {
                            _logger.LogError("[ProvisioningService_LoadRelatedEntities] Błąd integralności w '{SheetName}' wiersz {RowNumber}. Karta o ID={CardId} (kolumna '{HeaderName}') nie istnieje w arkuszu 'Cards'.", sheetName, row.RowNumber(), idFromExcel, headerName);
                            throw new InvalidDataException($"Błąd integralności w '{sheetName}' wiersz {row.RowNumber()}. Karta o ID={idFromExcel} (kolumna '{headerName}') nie istnieje w arkuszu 'Cards'.");
                        }
                    }
                    else if (processIdMap != null && string.Equals(propertyName, "Processes_Id", StringComparison.OrdinalIgnoreCase))
                    {
                        if (cell.TryGetValue(out int processId) && processIdMap.TryGetValue(processId, out var mappedProcessId)) prop.SetValue(entity, mappedProcessId);
                        else if (cell.TryGetValue(out processId))
                        {
                            _logger.LogError("[ProvisioningService_LoadRelatedEntities] Błąd integralności w '{SheetName}' wiersz {RowNumber}. Proces o ID={ProcessId} nie istnieje w arkuszu 'Processes'.", sheetName, row.RowNumber(), processId);
                            throw new InvalidDataException($"Błąd integralności w '{sheetName}' wiersz {row.RowNumber()}. Proces o ID={processId} nie istnieje w arkuszu 'Processes'.");
                        }
                    }
                    else if (string.Equals(propertyName, "Feedbacks_PDF", StringComparison.OrdinalIgnoreCase) && prop.PropertyType == typeof(byte[]))
                    {
                        var pdfPath = Path.Combine(AppContext.BaseDirectory, "Initializers", cell.GetString());
                        if (File.Exists(pdfPath))
                        {
                            prop.SetValue(entity, File.ReadAllBytes(pdfPath));
                            _logger.LogDebug("[ProvisioningService_LoadRelatedEntities] Załadowano plik PDF z '{PdfPath}' dla wiersza {RowNumber} w '{SheetName}'.", pdfPath, row.RowNumber(), sheetName);
                        }
                        else
                        {
                            _logger.LogWarning("[ProvisioningService_LoadRelatedEntities] Plik PDF '{PdfPath}' nie został znaleziony dla wiersza {RowNumber} w '{SheetName}'. Wartość będzie pusta.", pdfPath, row.RowNumber(), sheetName);
                        }
                    }
                    else
                    {
                        var valueToSet = ConvertValue(cell.GetString(), prop.PropertyType, cell.DataType);
                        var isNullable = Nullable.GetUnderlyingType(prop.PropertyType) != null || prop.PropertyType == typeof(string);
                        if (valueToSet == null && !isNullable)
                        {
                            prop.SetValue(entity, GetDefaultValue(prop.PropertyType));
                        }
                        else
                        {
                            prop.SetValue(entity, valueToSet);
                        }
                    }
                }

                entitiesToAdd.Add(entity);
            }

            if (entitiesToAdd.Any())
            {
                _logger.LogInformation("[ProvisioningService_LoadRelatedEntities] Przygotowano {Count} powiązanych encji '{EntityType}' z arkusza '{SheetName}' do dodania.", entitiesToAdd.Count, typeof(TEntity).Name, sheetName);
                context.Set<TEntity>().AddRange(entitiesToAdd);
            }
            else
            {
                _logger.LogWarning("[ProvisioningService_LoadRelatedEntities] Nie znaleziono powiązanych encji '{EntityType}' w arkuszu '{SheetName}'.", typeof(TEntity).Name, sheetName);
            }
        }

        private object? ConvertValue(string val, Type targetType, XLDataType dataType)
        {
            if (string.IsNullOrWhiteSpace(val)) return null;

            targetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            string normalizedVal = val.Replace(',', '.');

            if (targetType == typeof(bool))
            {
                if (val == "1") return true;
                if (val == "0") return false;
                if (bool.TryParse(val, out var result)) return result;
                return null;
            }

            if (targetType == typeof(int))
            {
                if (int.TryParse(normalizedVal, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)) return result;
                return null;
            }

            if (targetType == typeof(double))
            {
                if (double.TryParse(normalizedVal, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)) return result;
                return null;
            }

            if (targetType == typeof(DateTime))
            {
                if (DateTime.TryParse(val, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)) return result;
                return null;
            }

            return val;
        }

        private string? ConvertColorToHex(string colorValue, string sheetName, int rowNumber)
        {
            if (string.IsNullOrWhiteSpace(colorValue))
            {
                return null;
            }

            // Normalizacja: jeśli to hex bez '#' (3 lub 6 znaków), dodaj '#'
            if (!colorValue.StartsWith("#") && (colorValue.Length == 3 || colorValue.Length == 6) &&
                colorValue.All(c => (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f')))
            {
                colorValue = "#" + colorValue;
            }

            if (colorValue.StartsWith("#") && (colorValue.Length == 4 || colorValue.Length == 7))
            {
                return colorValue;
            }

            try
            {
                Color color = ColorTranslator.FromHtml(colorValue);
                return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            }
            catch (Exception)
            {
                var parts = colorValue.Replace(" ", "").Split(',');
                if (parts.Length == 3 &&
                    byte.TryParse(parts[0], out byte r) &&
                    byte.TryParse(parts[1], out byte g) &&
                    byte.TryParse(parts[2], out byte b))
                {
                    return $"#{r:X2}{g:X2}{b:X2}";
                }
            }

            _logger.LogWarning("[ProvisioningService_ConvertColorToHex] Nie można przekonwertować wartości koloru '{ColorValue}' na format HEX. Arkusz: '{SheetName}', Wiersz: {RowNumber}. Ustawiono wartość null.", colorValue, sheetName, rowNumber);
            return null;
        }

        private object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
            {
                return Activator.CreateInstance(t)!;
            }
            if (t == typeof(string))
            {
                return string.Empty;
            }
            return null!;
        }
    }
}