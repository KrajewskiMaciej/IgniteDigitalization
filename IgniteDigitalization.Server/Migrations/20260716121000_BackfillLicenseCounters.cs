using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IgniteDigitalization.Server.Migrations
{
    /// <inheritdoc />
    public partial class BackfillLicenseCounters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Jednorazowe wyrównanie liczników na istniejących danych do stanu z tabeli Games.
            //  - Licenses_Used = wszystkie gry usera (licencja = 1 gra, nie zwalniana po zakończeniu)
            //  - Games_In_Progress = gry nie-zakończone (during/paused)
            //  - Licenses_Owned: stary bug dekrementował pulę o 1 na każdą utworzoną grę.
            //    Migracja uruchamia się raz (EF), a przy świeżej bazie gier jeszcze nie ma,
            //    więc "+ liczba gier" odtwarza pierwotną pulę tylko dla danych ze starego kodu.
            migrationBuilder.Sql(@"
                UPDATE ""Users"" u SET
                    ""Licenses_Owned"" = u.""Licenses_Owned"" + (
                        SELECT COUNT(*) FROM ""Games"" g WHERE g.""Users_Id"" = u.""Users_Id""),
                    ""Licenses_Used"" = (
                        SELECT COUNT(*) FROM ""Games"" g WHERE g.""Users_Id"" = u.""Users_Id""),
                    ""Games_In_Progress"" = (
                        SELECT COUNT(*) FROM ""Games"" g
                        WHERE g.""Users_Id"" = u.""Users_Id"" AND g.""Game_Status"" IN ('during', 'paused'));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Backfill danych – brak sensownego cofnięcia.
        }
    }
}
