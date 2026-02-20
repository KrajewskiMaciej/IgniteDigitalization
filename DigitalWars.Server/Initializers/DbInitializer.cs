using backend.Data;
using backend.Services;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace backend.Initializers
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context, IProvisioningService provisioningService)
        {
            context.Database.Migrate();

            if (context.Users.Any()) return;

            var filePath = Path.Combine(AppContext.BaseDirectory, "Initializers", "DigitalWars_UserInitialization.xlsx");
            if (!File.Exists(filePath))
            {
                return;
            }

            // Pobierz dane startowego użytkownika z dedykowanej klasy
            var initialUser = InitialUserData.GetInitialUser();

            context.Users.Add(initialUser);
            context.SaveChanges();

            // Asynchronicznie zainicjalizuj zasoby (talię i plansze) dla nowego użytkownika
            Task.Run(async () => await provisioningService.InitializeNewUserAsync(initialUser.Users_Id)).Wait();
        }
    }
}