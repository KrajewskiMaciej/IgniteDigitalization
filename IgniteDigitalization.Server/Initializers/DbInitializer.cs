using backend.Data;
using backend.Services;
using Microsoft.EntityFrameworkCore;

namespace backend.Initializers
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context, IProvisioningService provisioningService)
        {
            context.Database.Migrate();

            // Auto-healing: uzupełnij brakujące Phases_Id kart (np. po zmianie import-logiki)
            Task.Run(async () => await provisioningService.HealExistingCardPhasesAsync()).Wait();

            if (context.Users.Any()) return;

            // Pobierz dane startowego użytkownika z dedykowanej klasy
            var initialUser = InitialUserData.GetInitialUser();

            context.Users.Add(initialUser);
            context.SaveChanges();

            // Zainicjalizuj plansze i wszystkie scenariusze (3 talie) dla pierwszego użytkownika
            Task.Run(async () => await provisioningService.InitializeNewUserAsync(initialUser.Users_Id)).Wait();
        }
    }
}