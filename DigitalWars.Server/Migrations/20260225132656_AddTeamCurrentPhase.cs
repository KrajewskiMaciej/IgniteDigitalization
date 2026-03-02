using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalWars.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamCurrentPhase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Current_Phase_Id",
                table: "Teams",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Current_Phase_Id",
                table: "Teams",
                column: "Current_Phase_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Phases_Current_Phase_Id",
                table: "Teams",
                column: "Current_Phase_Id",
                principalTable: "Phases",
                principalColumn: "Phases_Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Phases_Current_Phase_Id",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_Current_Phase_Id",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "Current_Phase_Id",
                table: "Teams");
        }
    }
}
