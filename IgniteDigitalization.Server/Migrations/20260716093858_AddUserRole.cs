using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IgniteDigitalization.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            // Istniejący Admin (już zaseedowana baza) — seeder się nie uruchomi ponownie, więc ustaw rolę 9 tutaj.
            migrationBuilder.Sql(
                "UPDATE \"Users\" SET \"Role\" = 9 WHERE \"Email\" = 'itmserwis@itm.com.pl';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");
        }
    }
}
