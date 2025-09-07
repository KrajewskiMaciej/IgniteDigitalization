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
                _logger.LogError("Brak pliku inicjalizacyjnego. Nie można zainicjalizować użytkownika.");
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

        // POPRAWIONA METODA
        /// <summary>
        /// Wczytuje tablice z pliku Excel i przypisuje je użytkownikowi, jeśli nie ma on jeszcze żadnych tablic.
        /// </summary>
        /// <param name="context">Kontekst bazy danych Entity Framework.</param>
        /// <param name="workbook">Obiekt XLWorkbook zawierający arkusz z szablonami tablic.</param>
        /// <param name="userId">Identyfikator użytkownika, dla którego mają zostać utworzone tablice.</param>
        private async Task SeedBoardsForUserFromFileAsync(AppDbContext context, XLWorkbook workbook, int userId)
        {
            // 1. Sprawdź, czy użytkownik posiada już jakiekolwiek tablice. Jeśli tak, przerwij operację.
            if (await context.Boards.AnyAsync(b => b.Users_Id == userId))
            {
                return;
            }

            // 2. Spróbuj uzyskać dostęp do arkusza o nazwie "Boards".
            var sheet = workbook.Worksheet("Boards");
            if (sheet == null)
            {
                // Opcjonalnie: można tutaj dodać logowanie informacji o braku arkusza.
                return;
            }

            var boardsToAdd = new List<Board>();

            // 3. Przejdź przez wszystkie używane wiersze w arkuszu, pomijając pierwszy (nagłówek).
            foreach (var row in sheet.RowsUsed().Skip(1))
            {
                // 4. Dla każdego wiersza z szablonem utwórz nową instancję Board,
                //    od razu przypisując jej docelowy identyfikator użytkownika (userId).
                boardsToAdd.Add(new Board
                {
                    Users_Id = userId, // Bezpośrednie przypisanie ID użytkownika
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
                    // Zauważ, że nie ma już potrzeby sprawdzania komórki z ID użytkownika w pliku Excel,
                    // ponieważ zakładamy, że cały ten arkusz służy jako źródło szablonów.
                });
            }

            // 5. Jeśli lista nowo utworzonych tablic nie jest pusta, dodaj je wszystkie
            //    do bazy danych w ramach jednej transakcji.
            if (boardsToAdd.Any())
            {
                context.Boards.AddRange(boardsToAdd);
                await context.SaveChangesAsync();
            }
        }

        private async Task<Dictionary<int, int>> LoadAndMapCardsAsync(AppDbContext context, XLWorkbook workbook, int deckId)
        {
            var idMap = new Dictionary<int, int>();
            var sheet = workbook.Worksheet("Cards");
            if (sheet == null) return idMap;

            foreach (var row in sheet.RowsUsed().Skip(1))
            {
                if (row.Cell(1).TryGetValue(out int cardIdFromExcel))
                {
                    var newCard = new Card
                    {
                        Decks_Id = deckId,
                        Card_Id = cardIdFromExcel,
                    };
                    context.Cards.Add(newCard);
                    await context.SaveChangesAsync();
                    idMap[cardIdFromExcel] = newCard.Cards_Id;
                }
            }
            return idMap;
        }

        private async Task<Dictionary<TKey, TKey>> LoadAndMapEntitiesAsync<TEntity, TKey>(AppDbContext context, XLWorkbook workbook, string sheetName, int deckId, string excelIdColumn, string dbIdColumn) where TEntity : class, new()
        {
            var idMap = new Dictionary<TKey, TKey>();
            var sheet = workbook.Worksheet(sheetName);
            if (sheet == null) return idMap;

            var properties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(p => p.Name, p => p);

            foreach (var row in sheet.RowsUsed().Skip(1))
            {
                var entity = new TEntity();
                properties["Decks_Id"]?.SetValue(entity, deckId);

                foreach (var cell in row.CellsUsed())
                {
                    var propName = sheet.Cell(1, cell.Address.ColumnNumber).GetString();
                    if (properties.TryGetValue(propName, out var prop))
                    {
                        prop.SetValue(entity, ConvertValue(cell.GetString(), prop.PropertyType, cell.DataType));
                    }
                }

                await context.Set<TEntity>().AddAsync(entity);
                await context.SaveChangesAsync();

                var excelId = (TKey)properties[excelIdColumn].GetValue(entity);
                var dbId = (TKey)properties[dbIdColumn].GetValue(entity);
                idMap[excelId] = dbId;
            }
            return idMap;
        }

        private void LoadRelatedEntities<TEntity>(AppDbContext context, XLWorkbook workbook, string sheetName, Dictionary<int, int> cardIdMap, Dictionary<int, int> processIdMap = null) where TEntity : class, new()
        {
            var sheet = workbook.Worksheet(sheetName);
            if (sheet == null) return;

            var properties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(p => p.Name, p => p);

            foreach (var row in sheet.RowsUsed().Skip(1))
            {
                var entity = new TEntity();

                foreach (var cell in row.CellsUsed())
                {
                    var propName = sheet.Cell(1, cell.Address.ColumnNumber).GetString();

                    if (propName == "CardId" || propName == "Card_Id") propName = "Cards_Id";
                    if (propName == "ProcessId") propName = "Processes_Id";

                    if (properties.TryGetValue(propName, out var prop))
                    {
                        if (propName == "Cards_Id" && cell.TryGetValue(out int cardId) && cardIdMap.TryGetValue(cardId, out var mappedCardId))
                        {
                            prop.SetValue(entity, mappedCardId);
                        }
                        else if (processIdMap != null && propName == "Processes_Id" && cell.TryGetValue(out int processId) && processIdMap.TryGetValue(processId, out var mappedProcessId))
                        {
                            prop.SetValue(entity, mappedProcessId);
                        }
                        else if (propName == "Feedbacks_PDF" && prop.PropertyType == typeof(byte[]))
                        {
                            var pdfPath = Path.Combine(AppContext.BaseDirectory, "Initializers", cell.GetString());
                            if (File.Exists(pdfPath))
                            {
                                prop.SetValue(entity, File.ReadAllBytes(pdfPath));
                            }
                        }
                        else
                        {
                            prop.SetValue(entity, ConvertValue(cell.GetString(), prop.PropertyType, cell.DataType));
                        }
                    }
                }
                context.Set<TEntity>().Add(entity);
            }
        }

        private object? ConvertValue(string val, Type targetType, XLDataType dataType)
        {
            if (string.IsNullOrWhiteSpace(val)) return null;
            targetType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            if (targetType == typeof(bool)) return bool.Parse(val);
            if (targetType == typeof(int)) return int.TryParse(val, CultureInfo.InvariantCulture, out int result) ? result : null;
            if (targetType == typeof(double)) return double.TryParse(val, CultureInfo.InvariantCulture, out double result) ? result : null;
            if (targetType == typeof(DateTime)) return DateTime.TryParse(val, CultureInfo.InvariantCulture, out DateTime result) ? result : null;
            return val;
        }
    }
}

