using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalWars.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Users_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Names = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Password = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email_Confirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Link_Token = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Token_Expire_Date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Licenses_Owned = table.Column<int>(type: "int", nullable: false),
                    Licenses_Used = table.Column<int>(type: "int", nullable: false),
                    Games_In_Progress = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Users_Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Boards",
                columns: table => new
                {
                    Boards_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Users_Id = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Labels_Up = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Labels_Right = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description_Down = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description_Left = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Rows = table.Column<int>(type: "int", nullable: false),
                    Cols = table.Column<int>(type: "int", nullable: false),
                    Border_Color = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cell_Color = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Borders_Colors = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Boards", x => x.Boards_Id);
                    table.ForeignKey(
                        name: "FK_Boards_Users_Users_Id",
                        column: x => x.Users_Id,
                        principalTable: "Users",
                        principalColumn: "Users_Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Decks",
                columns: table => new
                {
                    Decks_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Deck_Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Users_Id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Decks", x => x.Decks_Id);
                    table.ForeignKey(
                        name: "FK_Decks_Users_Users_Id",
                        column: x => x.Users_Id,
                        principalTable: "Users",
                        principalColumn: "Users_Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    Modules_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Decks_Id = table.Column<int>(type: "int", nullable: false),
                    Module_Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modules", x => x.Modules_Id);
                    table.ForeignKey(
                        name: "FK_Modules_Decks_Decks_Id",
                        column: x => x.Decks_Id,
                        principalTable: "Decks",
                        principalColumn: "Decks_Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    Cards_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Decks_Id = table.Column<int>(type: "int", nullable: false),
                    Modules_Id = table.Column<int>(type: "int", nullable: true),
                    Card_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.Cards_Id);
                    table.ForeignKey(
                        name: "FK_Cards_Decks_Decks_Id",
                        column: x => x.Decks_Id,
                        principalTable: "Decks",
                        principalColumn: "Decks_Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cards_Modules_Modules_Id",
                        column: x => x.Modules_Id,
                        principalTable: "Modules",
                        principalColumn: "Modules_Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GameEvents",
                columns: table => new
                {
                    Games_Events_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Decks_Id = table.Column<int>(type: "int", nullable: true),
                    Modules_Id = table.Column<int>(type: "int", nullable: true),
                    Events_Short_Desc = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Events_Long_Desc = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Turns_Time = table.Column<int>(type: "int", nullable: false),
                    Decisions_Costs_Bits_Weights = table.Column<double>(type: "double", nullable: true),
                    Decisions_Costs_PD_Weights = table.Column<double>(type: "double", nullable: true),
                    Hardwares_Costs_Bits_Weights = table.Column<double>(type: "double", nullable: true),
                    Hardwares_Costs_PD_Weights = table.Column<double>(type: "double", nullable: true),
                    Softwares_Costs_Bits_Weights = table.Column<double>(type: "double", nullable: true),
                    Softwares_Costs_PD_Weights = table.Column<double>(type: "double", nullable: true),
                    Boosters_X = table.Column<double>(type: "double", nullable: true),
                    Boosters_Y = table.Column<double>(type: "double", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameEvents", x => x.Games_Events_Id);
                    table.ForeignKey(
                        name: "FK_GameEvents_Decks_Decks_Id",
                        column: x => x.Decks_Id,
                        principalTable: "Decks",
                        principalColumn: "Decks_Id");
                    table.ForeignKey(
                        name: "FK_GameEvents_Modules_Modules_Id",
                        column: x => x.Modules_Id,
                        principalTable: "Modules",
                        principalColumn: "Modules_Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Games_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Games_Desc = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Teams_Boards_Id = table.Column<int>(type: "int", nullable: false),
                    Rivals_Boards_Id = table.Column<int>(type: "int", nullable: false),
                    Decks_Id = table.Column<int>(type: "int", nullable: false),
                    Modules_Id = table.Column<int>(type: "int", nullable: true),
                    Users_Id = table.Column<int>(type: "int", nullable: false),
                    Game_Status = table.Column<string>(type: "ENUM('During', 'Paused', 'End')", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Is_Online = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Games_Id);
                    table.ForeignKey(
                        name: "FK_Games_Boards_Rivals_Boards_Id",
                        column: x => x.Rivals_Boards_Id,
                        principalTable: "Boards",
                        principalColumn: "Boards_Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Games_Boards_Teams_Boards_Id",
                        column: x => x.Teams_Boards_Id,
                        principalTable: "Boards",
                        principalColumn: "Boards_Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Games_Decks_Decks_Id",
                        column: x => x.Decks_Id,
                        principalTable: "Decks",
                        principalColumn: "Decks_Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Games_Modules_Modules_Id",
                        column: x => x.Modules_Id,
                        principalTable: "Modules",
                        principalColumn: "Modules_Id");
                    table.ForeignKey(
                        name: "FK_Games_Users_Users_Id",
                        column: x => x.Users_Id,
                        principalTable: "Users",
                        principalColumn: "Users_Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Processes",
                columns: table => new
                {
                    Processes_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Processes_Desc = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Processes_Long_Desc = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Processes_Color = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Processes_Weight = table.Column<double>(type: "double", nullable: false),
                    Decks_Id = table.Column<int>(type: "int", nullable: false),
                    Modules_Id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Processes", x => x.Processes_Id);
                    table.ForeignKey(
                        name: "FK_Processes_Decks_Decks_Id",
                        column: x => x.Decks_Id,
                        principalTable: "Decks",
                        principalColumn: "Decks_Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Processes_Modules_Modules_Id",
                        column: x => x.Modules_Id,
                        principalTable: "Modules",
                        principalColumn: "Modules_Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Decisions",
                columns: table => new
                {
                    Decisions_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Cards_Id = table.Column<int>(type: "int", nullable: false),
                    Decisions_Short_Desc = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Decisions_Long_Desc = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Decisions_Cost_Bits = table.Column<double>(type: "double", nullable: false),
                    Decisions_Cost_Bits_Weight = table.Column<double>(type: "double", nullable: false),
                    Decisions_Cost_PD = table.Column<double>(type: "double", nullable: true),
                    Decisions_Cost_PD_Weight = table.Column<double>(type: "double", nullable: true),
                    Decisions_Reward_Bits = table.Column<double>(type: "double", nullable: true),
                    Decisions_Reward_Bits_Weight = table.Column<double>(type: "double", nullable: true),
                    Decisions_Reward_PD = table.Column<double>(type: "double", nullable: true),
                    Decisions_Reward_PD_Weight = table.Column<double>(type: "double", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Decisions", x => x.Decisions_Id);
                    table.ForeignKey(
                        name: "FK_Decisions_Cards_Cards_Id",
                        column: x => x.Cards_Id,
                        principalTable: "Cards",
                        principalColumn: "Cards_Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    Feedbacks_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Cards_Id = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Feedbacks_Long_Description = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Feedbacks_PDF = table.Column<byte[]>(type: "LONGBLOB", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.Feedbacks_Id);
                    table.ForeignKey(
                        name: "FK_Feedbacks_Cards_Cards_Id",
                        column: x => x.Cards_Id,
                        principalTable: "Cards",
                        principalColumn: "Cards_Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Hardwares",
                columns: table => new
                {
                    Hardwares_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Cards_Id = table.Column<int>(type: "int", nullable: false),
                    Hardwares_Short_Desc = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Hardwares_Long_Desc = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Hardwares_Cost_Bits = table.Column<double>(type: "double", nullable: false),
                    Hardwares_Cost_Bits_Weight = table.Column<double>(type: "double", nullable: false),
                    Hardwares_Cost_PD = table.Column<double>(type: "double", nullable: true),
                    Hardwares_Cost_PD_Weight = table.Column<double>(type: "double", nullable: true),
                    Hardwares_Reward_Bits = table.Column<double>(type: "double", nullable: true),
                    Hardwares_Reward_Bits_Weight = table.Column<double>(type: "double", nullable: true),
                    Hardwares_Reward_PD = table.Column<double>(type: "double", nullable: true),
                    Hardwares_Reward_PD_Weight = table.Column<double>(type: "double", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hardwares", x => x.Hardwares_Id);
                    table.ForeignKey(
                        name: "FK_Hardwares_Cards_Cards_Id",
                        column: x => x.Cards_Id,
                        principalTable: "Cards",
                        principalColumn: "Cards_Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Softwares",
                columns: table => new
                {
                    Softwares_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Cards_Id = table.Column<int>(type: "int", nullable: false),
                    Softwares_Short_Desc = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Softwares_Long_Desc = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Softwares_Cost_Bits = table.Column<double>(type: "double", nullable: false),
                    Softwares_Cost_Bits_Weight = table.Column<double>(type: "double", nullable: false),
                    Softwares_Cost_PD = table.Column<double>(type: "double", nullable: true),
                    Softwares_Cost_PD_Weight = table.Column<double>(type: "double", nullable: true),
                    Softwares_Reward_Bits = table.Column<double>(type: "double", nullable: true),
                    Softwares_Reward_Bits_Weight = table.Column<double>(type: "double", nullable: true),
                    Softwares_Reward_PD = table.Column<double>(type: "double", nullable: true),
                    Softwares_Reward_PD_Weight = table.Column<double>(type: "double", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Softwares", x => x.Softwares_Id);
                    table.ForeignKey(
                        name: "FK_Softwares_Cards_Cards_Id",
                        column: x => x.Cards_Id,
                        principalTable: "Cards",
                        principalColumn: "Cards_Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Teams_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Games_Id = table.Column<int>(type: "int", nullable: false),
                    Teams_Color = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Teams_Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Teams_Bud = table.Column<double>(type: "double", nullable: false),
                    Teams_PD = table.Column<double>(type: "double", nullable: true),
                    Teams_Token = table.Column<string>(type: "varchar(6)", maxLength: 6, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Games_Events_Id = table.Column<int>(type: "int", nullable: true),
                    Turns_Left = table.Column<int>(type: "int", nullable: true),
                    Is_Independent = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Teams_Id);
                    table.ForeignKey(
                        name: "FK_Teams_GameEvents_Games_Events_Id",
                        column: x => x.Games_Events_Id,
                        principalTable: "GameEvents",
                        principalColumn: "Games_Events_Id");
                    table.ForeignKey(
                        name: "FK_Teams_Games_Games_Id",
                        column: x => x.Games_Id,
                        principalTable: "Games",
                        principalColumn: "Games_Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CardWeights",
                columns: table => new
                {
                    Cards_Weights_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Cards_Id = table.Column<int>(type: "int", nullable: false),
                    Processes_Id = table.Column<int>(type: "int", nullable: false),
                    Weights_X = table.Column<int>(type: "int", nullable: false),
                    Weights_Y = table.Column<int>(type: "int", nullable: false),
                    Booster_X = table.Column<double>(type: "double", nullable: false),
                    Booster_Y = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardWeights", x => x.Cards_Weights_Id);
                    table.ForeignKey(
                        name: "FK_CardWeights_Cards_Cards_Id",
                        column: x => x.Cards_Id,
                        principalTable: "Cards",
                        principalColumn: "Cards_Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CardWeights_Processes_Processes_Id",
                        column: x => x.Processes_Id,
                        principalTable: "Processes",
                        principalColumn: "Processes_Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CardEnablers",
                columns: table => new
                {
                    Cards_Enablers_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Cards_Id = table.Column<int>(type: "int", nullable: false),
                    Enablers_Id = table.Column<int>(type: "int", nullable: true),
                    Games_Id = table.Column<int>(type: "int", nullable: true),
                    Teams_Id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardEnablers", x => x.Cards_Enablers_Id);
                    table.ForeignKey(
                        name: "FK_CardEnablers_Cards_Cards_Id",
                        column: x => x.Cards_Id,
                        principalTable: "Cards",
                        principalColumn: "Cards_Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CardEnablers_Cards_Enablers_Id",
                        column: x => x.Enablers_Id,
                        principalTable: "Cards",
                        principalColumn: "Cards_Id");
                    table.ForeignKey(
                        name: "FK_CardEnablers_Games_Games_Id",
                        column: x => x.Games_Id,
                        principalTable: "Games",
                        principalColumn: "Games_Id");
                    table.ForeignKey(
                        name: "FK_CardEnablers_Teams_Teams_Id",
                        column: x => x.Teams_Id,
                        principalTable: "Teams",
                        principalColumn: "Teams_Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GameLogs",
                columns: table => new
                {
                    Games_Logs_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Data = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Teams_Id = table.Column<int>(type: "int", nullable: true),
                    Games_Id = table.Column<int>(type: "int", nullable: false),
                    Games_Events_Id = table.Column<int>(type: "int", nullable: true),
                    Cards_Id = table.Column<int>(type: "int", nullable: true),
                    Boards_Id = table.Column<int>(type: "int", nullable: true),
                    Feedbacks_Id = table.Column<int>(type: "int", nullable: true),
                    Costs = table.Column<double>(type: "double", nullable: true),
                    Status = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    Is_Approved = table.Column<bool>(type: "tinyint(1)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameLogs", x => x.Games_Logs_Id);
                    table.ForeignKey(
                        name: "FK_GameLogs_Boards_Boards_Id",
                        column: x => x.Boards_Id,
                        principalTable: "Boards",
                        principalColumn: "Boards_Id");
                    table.ForeignKey(
                        name: "FK_GameLogs_Cards_Cards_Id",
                        column: x => x.Cards_Id,
                        principalTable: "Cards",
                        principalColumn: "Cards_Id");
                    table.ForeignKey(
                        name: "FK_GameLogs_Feedbacks_Feedbacks_Id",
                        column: x => x.Feedbacks_Id,
                        principalTable: "Feedbacks",
                        principalColumn: "Feedbacks_Id");
                    table.ForeignKey(
                        name: "FK_GameLogs_GameEvents_Games_Events_Id",
                        column: x => x.Games_Events_Id,
                        principalTable: "GameEvents",
                        principalColumn: "Games_Events_Id");
                    table.ForeignKey(
                        name: "FK_GameLogs_Games_Games_Id",
                        column: x => x.Games_Id,
                        principalTable: "Games",
                        principalColumn: "Games_Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameLogs_Teams_Teams_Id",
                        column: x => x.Teams_Id,
                        principalTable: "Teams",
                        principalColumn: "Teams_Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GameProcesses",
                columns: table => new
                {
                    Games_Processes_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Processes_Id = table.Column<int>(type: "int", nullable: false),
                    Games_Id = table.Column<int>(type: "int", nullable: false),
                    Teams_Id = table.Column<int>(type: "int", nullable: false),
                    Games_Processes_Weights = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameProcesses", x => x.Games_Processes_Id);
                    table.ForeignKey(
                        name: "FK_GameProcesses_Games_Games_Id",
                        column: x => x.Games_Id,
                        principalTable: "Games",
                        principalColumn: "Games_Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameProcesses_Processes_Processes_Id",
                        column: x => x.Processes_Id,
                        principalTable: "Processes",
                        principalColumn: "Processes_Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameProcesses_Teams_Teams_Id",
                        column: x => x.Teams_Id,
                        principalTable: "Teams",
                        principalColumn: "Teams_Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GameBoards",
                columns: table => new
                {
                    Games_Boards_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Teams_Id = table.Column<int>(type: "int", nullable: false),
                    Games_Id = table.Column<int>(type: "int", nullable: false),
                    Games_Processes_Id = table.Column<int>(type: "int", nullable: true),
                    Poz_X = table.Column<double>(type: "double", nullable: false),
                    Poz_Y = table.Column<double>(type: "double", nullable: false),
                    Boards_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameBoards", x => x.Games_Boards_Id);
                    table.ForeignKey(
                        name: "FK_GameBoards_Boards_Boards_Id",
                        column: x => x.Boards_Id,
                        principalTable: "Boards",
                        principalColumn: "Boards_Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameBoards_GameProcesses_Games_Processes_Id",
                        column: x => x.Games_Processes_Id,
                        principalTable: "GameProcesses",
                        principalColumn: "Games_Processes_Id");
                    table.ForeignKey(
                        name: "FK_GameBoards_Games_Games_Id",
                        column: x => x.Games_Id,
                        principalTable: "Games",
                        principalColumn: "Games_Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameBoards_Teams_Teams_Id",
                        column: x => x.Teams_Id,
                        principalTable: "Teams",
                        principalColumn: "Teams_Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GameLogSpecs",
                columns: table => new
                {
                    Games_Logs_Specs_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Games_Logs_Id = table.Column<int>(type: "int", nullable: false),
                    Games_Processes_Id = table.Column<int>(type: "int", nullable: true),
                    Moves_X = table.Column<int>(type: "int", nullable: false),
                    Moves_Y = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameLogSpecs", x => x.Games_Logs_Specs_Id);
                    table.ForeignKey(
                        name: "FK_GameLogSpecs_GameLogs_Games_Logs_Id",
                        column: x => x.Games_Logs_Id,
                        principalTable: "GameLogs",
                        principalColumn: "Games_Logs_Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameLogSpecs_GameProcesses_Games_Processes_Id",
                        column: x => x.Games_Processes_Id,
                        principalTable: "GameProcesses",
                        principalColumn: "Games_Processes_Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Boards_Users_Id",
                table: "Boards",
                column: "Users_Id");

            migrationBuilder.CreateIndex(
                name: "IX_CardEnablers_Cards_Id",
                table: "CardEnablers",
                column: "Cards_Id");

            migrationBuilder.CreateIndex(
                name: "IX_CardEnablers_Enablers_Id",
                table: "CardEnablers",
                column: "Enablers_Id");

            migrationBuilder.CreateIndex(
                name: "IX_CardEnablers_Games_Id",
                table: "CardEnablers",
                column: "Games_Id");

            migrationBuilder.CreateIndex(
                name: "IX_CardEnablers_Teams_Id",
                table: "CardEnablers",
                column: "Teams_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_Decks_Id",
                table: "Cards",
                column: "Decks_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_Modules_Id",
                table: "Cards",
                column: "Modules_Id");

            migrationBuilder.CreateIndex(
                name: "IX_CardWeights_Cards_Id",
                table: "CardWeights",
                column: "Cards_Id");

            migrationBuilder.CreateIndex(
                name: "IX_CardWeights_Processes_Id",
                table: "CardWeights",
                column: "Processes_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Decisions_Cards_Id",
                table: "Decisions",
                column: "Cards_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Decks_Users_Id",
                table: "Decks",
                column: "Users_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_Cards_Id",
                table: "Feedbacks",
                column: "Cards_Id");

            migrationBuilder.CreateIndex(
                name: "IX_GameBoards_Boards_Id",
                table: "GameBoards",
                column: "Boards_Id");

            migrationBuilder.CreateIndex(
                name: "IX_GameBoards_Games_Id",
                table: "GameBoards",
                column: "Games_Id");

            migrationBuilder.CreateIndex(
                name: "IX_GameBoards_Games_Processes_Id",
                table: "GameBoards",
                column: "Games_Processes_Id");

            migrationBuilder.CreateIndex(
                name: "IX_GameBoards_Teams_Id",
                table: "GameBoards",
                column: "Teams_Id");

            migrationBuilder.CreateIndex(
                name: "IX_GameEvents_Decks_Id",
                table: "GameEvents",
                column: "Decks_Id");

            migrationBuilder.CreateIndex(
                name: "IX_GameEvents_Modules_Id",
                table: "GameEvents",
                column: "Modules_Id");

            migrationBuilder.CreateIndex(
                name: "IX_GameLogs_Boards_Id",
                table: "GameLogs",
                column: "Boards_Id");

            migrationBuilder.CreateIndex(
                name: "IX_GameLogs_Cards_Id",
                table: "GameLogs",
                column: "Cards_Id");

            migrationBuilder.CreateIndex(
                name: "IX_GameLogs_Feedbacks_Id",
                table: "GameLogs",
                column: "Feedbacks_Id");

            migrationBuilder.CreateIndex(
                name: "IX_GameLogs_Games_Events_Id",
                table: "GameLogs",
                column: "Games_Events_Id");

            migrationBuilder.CreateIndex(
                name: "IX_GameLogs_Games_Id",
                table: "GameLogs",
                column: "Games_Id");

            migrationBuilder.CreateIndex(
                name: "IX_GameLogs_Teams_Id",
                table: "GameLogs",
                column: "Teams_Id");

            migrationBuilder.CreateIndex(
                name: "IX_GameLogSpecs_Games_Logs_Id",
                table: "GameLogSpecs",
                column: "Games_Logs_Id");

            migrationBuilder.CreateIndex(
                name: "IX_GameLogSpecs_Games_Processes_Id",
                table: "GameLogSpecs",
                column: "Games_Processes_Id");

            migrationBuilder.CreateIndex(
                name: "IX_GameProcesses_Games_Id",
                table: "GameProcesses",
                column: "Games_Id");

            migrationBuilder.CreateIndex(
                name: "IX_GameProcesses_Processes_Id",
                table: "GameProcesses",
                column: "Processes_Id");

            migrationBuilder.CreateIndex(
                name: "IX_GameProcesses_Teams_Id",
                table: "GameProcesses",
                column: "Teams_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Games_Decks_Id",
                table: "Games",
                column: "Decks_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Games_Modules_Id",
                table: "Games",
                column: "Modules_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Games_Rivals_Boards_Id",
                table: "Games",
                column: "Rivals_Boards_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Games_Teams_Boards_Id",
                table: "Games",
                column: "Teams_Boards_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Games_Users_Id",
                table: "Games",
                column: "Users_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Hardwares_Cards_Id",
                table: "Hardwares",
                column: "Cards_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_Decks_Id",
                table: "Modules",
                column: "Decks_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Processes_Decks_Id",
                table: "Processes",
                column: "Decks_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Processes_Modules_Id",
                table: "Processes",
                column: "Modules_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Softwares_Cards_Id",
                table: "Softwares",
                column: "Cards_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Games_Events_Id",
                table: "Teams",
                column: "Games_Events_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Games_Id",
                table: "Teams",
                column: "Games_Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardEnablers");

            migrationBuilder.DropTable(
                name: "CardWeights");

            migrationBuilder.DropTable(
                name: "Decisions");

            migrationBuilder.DropTable(
                name: "GameBoards");

            migrationBuilder.DropTable(
                name: "GameLogSpecs");

            migrationBuilder.DropTable(
                name: "Hardwares");

            migrationBuilder.DropTable(
                name: "Softwares");

            migrationBuilder.DropTable(
                name: "GameLogs");

            migrationBuilder.DropTable(
                name: "GameProcesses");

            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "Processes");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Cards");

            migrationBuilder.DropTable(
                name: "GameEvents");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "Boards");

            migrationBuilder.DropTable(
                name: "Modules");

            migrationBuilder.DropTable(
                name: "Decks");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
