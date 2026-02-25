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

                await SeedBoardsForUserFromFileAsync(context, workbook, userId);
                await CreateDeckFromWorkbookAsync(context, workbook, userId, "Talia podstawowa");

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

            return await CreateDeckFromWorkbookAsync(context, workbook, userId, deckName);
        }

        private async Task<Deck> CreateDeckFromWorkbookAsync(AppDbContext context, XLWorkbook workbook, int? userId, string deckName)
        {
            _logger.LogInformation("[ProvisioningService_CreateDeckFromWorkbookAsync] Rozpoczynanie wewnętrznego procesu tworzenia talii '{DeckName}' dla użytkownika {UserId}", deckName, userId);

            ValidateWorkbookStructure(workbook);
            _logger.LogInformation("[ProvisioningService_CreateDeckFromWorkbookAsync] Struktura pliku Excel została pomyślnie zweryfikowana.");

            await using var transaction = await context.Database.BeginTransactionAsync();
            _logger.LogDebug("[ProvisioningService_CreateDeckFromWorkbookAsync] Rozpoczęto transakcję w bazie danych.");
            try
            {
                var newDeck = new Deck { Deck_Name = deckName, Users_Id = userId };
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

        private async Task SeedBoardsForUserFromFileAsync(AppDbContext context, XLWorkbook workbook, int userId)
        {
            _logger.LogInformation("[ProvisioningService_SeedBoardsForUserFromFileAsync] Rozpoczynanie seedowania plansz dla użytkownika {UserId}", userId);
            if (await context.Boards.AnyAsync(b => b.Users_Id == userId))
            {
                _logger.LogInformation("[ProvisioningService_SeedBoardsForUserFromFileAsync] Plansze dla użytkownika {UserId} już istnieją. Pomijanie seedowania.", userId);
                return;
            }

            if (!workbook.TryGetWorksheet("Boards", out var sheet))
            {
                _logger.LogWarning("[ProvisioningService_SeedBoardsForUserFromFileAsync] Arkusz 'Boards' nie został znaleziony w pliku. Nie można zainicjalizować plansz.");
                return;
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
                    Borders_Colors = row.Cell(10).GetString()
                });
            }

            if (boardsToAdd.Any())
            {
                _logger.LogInformation("[ProvisioningService_SeedBoardsForUserFromFileAsync] Znaleziono {Count} plansz do dodania dla użytkownika {UserId}.", boardsToAdd.Count, userId);
                context.Boards.AddRange(boardsToAdd);
                await context.SaveChangesAsync();
                _logger.LogInformation("[ProvisioningService_SeedBoardsForUserFromFileAsync] Pomyślnie dodano {Count} plansz dla użytkownika {UserId}.", boardsToAdd.Count, userId);
            }
            else
            {
                _logger.LogWarning("[ProvisioningService_SeedBoardsForUserFromFileAsync] Arkusz 'Boards' został znaleziony, ale jest pusty. Nie dodano żadnych plansz dla użytkownika {UserId}.", userId);
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