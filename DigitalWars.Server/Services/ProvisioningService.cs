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
            _logger.LogInformation("Rozpoczynanie inicjalizacji danych dla nowego użytkownika {UserId}", userId);
            var filePath = Path.Combine(AppContext.BaseDirectory, "Initializers", "DigitalWars_UserInitialization.xlsx");
            if (!File.Exists(filePath))
            {
                _logger.LogError("Brak pliku inicjalacyjnego. Nie można zainicjalizować użytkownika.");
                return;
            }
            using var workbook = new XLWorkbook(filePath);
            await SeedBoardsForUserFromFileAsync(context, workbook, userId);
            await CreateDeckFromWorkbookAsync(context, workbook, userId, "Talia podstawowa");
            _logger.LogInformation("Zakończono inicjalizację danych dla użytkownika {UserId}.", userId);
        }

        public async Task<Deck> CreateDeckFromFileForUserAsync(IFormFile file, int userId, string deckName)
        {
            if (file == null || file.Length == 0) throw new ArgumentException("Nie przesłano pliku.");
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;
            using var workbook = new XLWorkbook(stream);
            return await CreateDeckFromWorkbookAsync(context, workbook, userId, deckName);
        }

        private async Task<Deck> CreateDeckFromWorkbookAsync(AppDbContext context, XLWorkbook workbook, int? userId, string deckName)
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var newDeck = new Deck { Deck_Name = deckName, Users_Id = userId };
                context.Decks.Add(newDeck);
                await context.SaveChangesAsync();

                var cardIdMap = await LoadAndMapCardsAsync(context, workbook, newDeck.Decks_Id);
                var processIdMap = await LoadAndMapEntitiesAsync<Process, int>(context, workbook, "Processes", newDeck.Decks_Id, "Processes_Id", "Processes_Id");

                await LoadEntitiesWithoutIdMappingAsync<GameEvent>(context, workbook, "GamesEvents", newDeck.Decks_Id);

                LoadRelatedEntities<Decision>(context, workbook, "Decisions", cardIdMap);
                LoadRelatedEntities<Hardware>(context, workbook, "Hardwares", cardIdMap);
                LoadRelatedEntities<Software>(context, workbook, "Softwares", cardIdMap);
                LoadRelatedEntities<Feedback>(context, workbook, "Feedbacks", cardIdMap);
                LoadRelatedEntities<CardWeight>(context, workbook, "CardsWeights", cardIdMap, processIdMap);
                LoadRelatedEntities<CardEnabler>(context, workbook, "CardsEnablers", cardIdMap);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Pomyślnie utworzono talię '{DeckName}' dla użytkownika {UserId}", deckName, userId);
                return newDeck;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Błąd podczas tworzenia talii z pliku Excel.");
                throw;
            }
        }

        private async Task SeedBoardsForUserFromFileAsync(AppDbContext context, XLWorkbook workbook, int userId)
        {
            if (await context.Boards.AnyAsync(b => b.Users_Id == userId)) return;
            var sheet = workbook.Worksheet("Boards");
            if (sheet == null) return;
            var boardsToAdd = new List<Board>();
            foreach (var row in sheet.RowsUsed().Skip(1))
            {
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
                context.Boards.AddRange(boardsToAdd);
                await context.SaveChangesAsync();
            }
        }

        private async Task LoadEntitiesWithoutIdMappingAsync<TEntity>(AppDbContext context, XLWorkbook workbook, string sheetName, int deckId) where TEntity : class, new()
        {
            var sheet = workbook.Worksheet(sheetName);
            if (sheet == null)
            {
                _logger.LogWarning("Arkusz '{SheetName}' nie został znaleziony, pomijanie.", sheetName);
                return;
            }

            var properties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);
            var headers = sheet.Row(1).CellsUsed().ToDictionary(cell => cell.Address.ColumnNumber, cell => cell.GetString());
            var entitiesToAdd = new List<TEntity>();

            foreach (var row in sheet.RowsUsed().Skip(1))
            {
                var entity = new TEntity();
                properties.GetValueOrDefault("Decks_Id")?.SetValue(entity, deckId);

                foreach (var cell in row.CellsUsed())
                {
                    if (headers.TryGetValue(cell.Address.ColumnNumber, out var propName) && properties.TryGetValue(propName, out var prop))
                    {
                        prop.SetValue(entity, ConvertValue(cell.GetString(), prop.PropertyType, cell.DataType));
                    }
                }
                entitiesToAdd.Add(entity);
            }

            if (entitiesToAdd.Any())
            {
                await context.Set<TEntity>().AddRangeAsync(entitiesToAdd);
                _logger.LogInformation("Przygotowano {Count} encji typu '{EntityType}' z arkusza '{SheetName}' do dodania.", entitiesToAdd.Count, typeof(TEntity).Name, sheetName);
            }
        }

        private async Task<Dictionary<int, int>> LoadAndMapCardsAsync(AppDbContext context, XLWorkbook workbook, int deckId)
        {
            var sheet = workbook.Worksheet("Cards");
            if (sheet == null)
            {
                _logger.LogWarning("Nie znaleziono arkusza 'Cards' w pliku Excel.");
                return new Dictionary<int, int>();
            }

            var cardsToAdd = new List<Card>();
            foreach (var row in sheet.RowsUsed().Skip(1))
            {
                if (row.Cell(1).TryGetValue(out int cardIdFromExcel))
                {
                    cardsToAdd.Add(new Card
                    {
                        Decks_Id = deckId,
                        Card_Id = cardIdFromExcel,
                    });
                }
            }

            if (!cardsToAdd.Any())
            {
                _logger.LogWarning("Arkusz 'Cards' jest pusty lub nie zawiera prawidłowych ID kart.");
                return new Dictionary<int, int>();
            }

            context.Cards.AddRange(cardsToAdd);
            await context.SaveChangesAsync();

            var idMap = cardsToAdd.ToDictionary(
                card => card.Card_Id,
                card => card.Cards_Id
            );

            return idMap;
        }

        private async Task<Dictionary<TKey, TKey>> LoadAndMapEntitiesAsync<TEntity, TKey>(AppDbContext context, XLWorkbook workbook, string sheetName, int deckId, string excelIdColumn, string dbIdColumn) where TEntity : class, new() where TKey : notnull
        {
            var sheet = workbook.Worksheet(sheetName);
            if (sheet == null)
            {
                _logger.LogWarning("Arkusz '{SheetName}' nie został znaleziony, pomijanie.", sheetName);
                return new Dictionary<TKey, TKey>();
            }

            var properties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);
            var headers = sheet.Row(1).CellsUsed().ToDictionary(cell => cell.Address.ColumnNumber, cell => cell.GetString());
            var entitiesWithExcelId = new List<(TEntity entity, TKey excelId)>();

            foreach (var row in sheet.RowsUsed().Skip(1))
            {
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
                        var convertedValue = ConvertValue(cell.GetString(), prop.PropertyType, cell.DataType);
                        prop.SetValue(entity, convertedValue);
                    }
                }
                entitiesWithExcelId.Add((entity, excelIdValue));
            }

            if (entitiesWithExcelId.Any())
            {
                await context.Set<TEntity>().AddRangeAsync(entitiesWithExcelId.Select(t => t.entity));
                await context.SaveChangesAsync();

                var dbIdProperty = properties[dbIdColumn];
                return entitiesWithExcelId.ToDictionary(
                    t => t.excelId,
                    t => (TKey)dbIdProperty.GetValue(t.entity)!
                );
            }

            return new Dictionary<TKey, TKey>();
        }

        private void LoadRelatedEntities<TEntity>(AppDbContext context, XLWorkbook workbook, string sheetName, Dictionary<int, int> cardIdMap, Dictionary<int, int>? processIdMap = null) where TEntity : class, new()
        {
            var sheet = workbook.Worksheet(sheetName);
            if (sheet == null) return;
            var properties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

            var cardForeignKeyPropertyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Cards_Id", "Enablers_Id" };

            var headers = sheet.Row(1).CellsUsed().ToDictionary(cell => cell.Address.ColumnNumber, cell => cell.GetString());

            var primaryKeyColumnName = sheetName.TrimEnd('s') + "_Id";
            var primaryKeyColumnNamePlural = sheetName + "_Id";

            foreach (var row in sheet.RowsUsed().Skip(1))
            {
                var entity = new TEntity();
                bool isPrimaryCardIdSet = false;
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
                            if (string.Equals(propertyName, "Cards_Id", StringComparison.OrdinalIgnoreCase)) isPrimaryCardIdSet = true;
                        }
                        else if (cell.TryGetValue(out idFromExcel)) throw new InvalidDataException($"Błąd integralności w '{sheetName}' wiersz {row.RowNumber()}. Karta o ID={idFromExcel} (kolumna '{headerName}') nie istnieje w arkuszu 'Cards'.");
                    }
                    else if (processIdMap != null && string.Equals(propertyName, "Processes_Id", StringComparison.OrdinalIgnoreCase))
                    {
                        if (cell.TryGetValue(out int processId) && processIdMap.TryGetValue(processId, out var mappedProcessId)) prop.SetValue(entity, mappedProcessId);
                        else if (cell.TryGetValue(out processId)) throw new InvalidDataException($"Błąd integralności w '{sheetName}' wiersz {row.RowNumber()}. Proces o ID={processId} nie istnieje w arkuszu 'Processes'.");
                    }
                    else if (string.Equals(propertyName, "Feedbacks_PDF", StringComparison.OrdinalIgnoreCase) && prop.PropertyType == typeof(byte[]))
                    {
                        var pdfPath = Path.Combine(AppContext.BaseDirectory, "Initializers", cell.GetString());
                        if (File.Exists(pdfPath)) prop.SetValue(entity, File.ReadAllBytes(pdfPath));
                    }
                    else prop.SetValue(entity, ConvertValue(cell.GetString(), prop.PropertyType, cell.DataType));
                }
                if (properties.ContainsKey("Cards_Id") && !isPrimaryCardIdSet) throw new InvalidDataException($"Błąd integralności w '{sheetName}' wiersz {row.RowNumber()}. Brak wartości lub nieprawidłowa wartość w kolumnie 'Cards_Id' (lub 'Card_Id').");
                context.Set<TEntity>().Add(entity);
            }
        }

        /// <summary>
        /// Poprawiona metoda, która poprawnie konwertuje liczby ujemne i zmiennoprzecinkowe.
        /// </summary>
        private object? ConvertValue(string val, Type targetType, XLDataType dataType)
        {
            if (string.IsNullOrWhiteSpace(val)) return null;

            targetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            // Normalizujemy string, zamieniając przecinek na kropkę, aby zapewnić spójne parsowanie liczb.
            string normalizedVal = val.Replace(',', '.');

            if (targetType == typeof(bool))
            {
                if (val == "1") return true;
                if (val == "0") return false;
                return bool.TryParse(val, out var result) ? result : null;
            }

            if (targetType == typeof(int))
            {
                // NumberStyles.Integer pozwala na znak wiodący (np. "-"), co zapewnia obsługę liczb ujemnych.
                return int.TryParse(normalizedVal, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : null;
            }

            if (targetType == typeof(double))
            {
                // NumberStyles.Float pozwala na znak wiodący i separator dziesiętny.
                // Dzięki normalizacji, CultureInfo.InvariantCulture poprawnie zinterpretuje kropkę.
                return double.TryParse(normalizedVal, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : null;
            }

            if (targetType == typeof(DateTime))
            {
                // Próbujemy sparsować datę, obsługując kilka popularnych formatów
                return DateTime.TryParse(val, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result) ? result : null;
            }

            return val;
        }
    }
}