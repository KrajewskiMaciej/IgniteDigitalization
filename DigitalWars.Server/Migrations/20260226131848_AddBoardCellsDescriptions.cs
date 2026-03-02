using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalWars.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddBoardCellsDescriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cells_Descriptions",
                table: "Boards",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cells_Descriptions",
                table: "Boards");
        }
    }
}
