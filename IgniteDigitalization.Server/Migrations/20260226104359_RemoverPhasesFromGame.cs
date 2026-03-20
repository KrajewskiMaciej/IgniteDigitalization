using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IgniteDigitalization.Server.Migrations
{
    /// <inheritdoc />
    public partial class RemoverPhasesFromGame : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_Phases_Phases_Id",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_Games_Phases_Id",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Phases_Id",
                table: "Games");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Phases_Id",
                table: "Games",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Games_Phases_Id",
                table: "Games",
                column: "Phases_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Phases_Phases_Id",
                table: "Games",
                column: "Phases_Id",
                principalTable: "Phases",
                principalColumn: "Phases_Id");
        }
    }
}
