using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IgniteDigitalization.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddDeckEconomyAndDefaultBoards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Default_Rivals_Boards_Id",
                table: "Decks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Default_Teams_Boards_Id",
                table: "Decks",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DeckEconomySettings",
                columns: table => new
                {
                    DeckEconomySettings_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Decks_Id = table.Column<int>(type: "integer", nullable: false),
                    Map1_Starting_Budget = table.Column<double>(type: "double precision", nullable: false),
                    Map1_Mandatory_Cards_Cost = table.Column<double>(type: "double precision", nullable: false),
                    Map1_Target_Cards_Min = table.Column<int>(type: "integer", nullable: false),
                    Map1_Target_Cards_Max = table.Column<int>(type: "integer", nullable: false),
                    Map2_Base_Budget = table.Column<double>(type: "double precision", nullable: false),
                    Map2_Prep_Bonus_Max_Bits = table.Column<double>(type: "double precision", nullable: false),
                    Map2_Prep_Cards_Total_Count = table.Column<int>(type: "integer", nullable: false),
                    Map2_Target_Cards_Min = table.Column<int>(type: "integer", nullable: false),
                    Map2_Target_Cards_Max = table.Column<int>(type: "integer", nullable: false),
                    PrepMultiplier_Max = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeckEconomySettings", x => x.DeckEconomySettings_Id);
                    table.ForeignKey(
                        name: "FK_DeckEconomySettings_Decks_Decks_Id",
                        column: x => x.Decks_Id,
                        principalTable: "Decks",
                        principalColumn: "Decks_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Decks_Default_Rivals_Boards_Id",
                table: "Decks",
                column: "Default_Rivals_Boards_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Decks_Default_Teams_Boards_Id",
                table: "Decks",
                column: "Default_Teams_Boards_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DeckEconomySettings_Decks_Id",
                table: "DeckEconomySettings",
                column: "Decks_Id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Decks_Boards_Default_Rivals_Boards_Id",
                table: "Decks",
                column: "Default_Rivals_Boards_Id",
                principalTable: "Boards",
                principalColumn: "Boards_Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Decks_Boards_Default_Teams_Boards_Id",
                table: "Decks",
                column: "Default_Teams_Boards_Id",
                principalTable: "Boards",
                principalColumn: "Boards_Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Decks_Boards_Default_Rivals_Boards_Id",
                table: "Decks");

            migrationBuilder.DropForeignKey(
                name: "FK_Decks_Boards_Default_Teams_Boards_Id",
                table: "Decks");

            migrationBuilder.DropTable(
                name: "DeckEconomySettings");

            migrationBuilder.DropIndex(
                name: "IX_Decks_Default_Rivals_Boards_Id",
                table: "Decks");

            migrationBuilder.DropIndex(
                name: "IX_Decks_Default_Teams_Boards_Id",
                table: "Decks");

            migrationBuilder.DropColumn(
                name: "Default_Rivals_Boards_Id",
                table: "Decks");

            migrationBuilder.DropColumn(
                name: "Default_Teams_Boards_Id",
                table: "Decks");
        }
    }
}
