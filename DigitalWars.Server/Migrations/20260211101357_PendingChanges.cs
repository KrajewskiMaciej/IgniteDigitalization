using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalWars.Server.Migrations
{
    /// <inheritdoc />
    public partial class PendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameLogs_Teams_Teams_Id",
                table: "GameLogs");

            migrationBuilder.AlterColumn<int>(
                name: "Teams_Id",
                table: "GameLogs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GameLogs_Teams_Teams_Id",
                table: "GameLogs",
                column: "Teams_Id",
                principalTable: "Teams",
                principalColumn: "Teams_Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameLogs_Teams_Teams_Id",
                table: "GameLogs");

            migrationBuilder.AlterColumn<int>(
                name: "Teams_Id",
                table: "GameLogs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_GameLogs_Teams_Teams_Id",
                table: "GameLogs",
                column: "Teams_Id",
                principalTable: "Teams",
                principalColumn: "Teams_Id");
        }
    }
}
