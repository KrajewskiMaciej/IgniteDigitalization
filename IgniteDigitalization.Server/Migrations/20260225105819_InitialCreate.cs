using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IgniteDigitalization.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Users_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Names = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    Email_Confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    Link_Token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Token_Expire_Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Licenses_Owned = table.Column<int>(type: "integer", nullable: false),
                    Licenses_Used = table.Column<int>(type: "integer", nullable: false),
                    Games_In_Progress = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Users_Id);
                });

            migrationBuilder.CreateTable(
                name: "Boards",
                columns: table => new
                {
                    Boards_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Users_Id = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Labels_Up = table.Column<string>(type: "TEXT", nullable: false),
                    Labels_Right = table.Column<string>(type: "TEXT", nullable: false),
                    Description_Down = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description_Left = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Rows = table.Column<int>(type: "integer", nullable: false),
                    Cols = table.Column<int>(type: "integer", nullable: false),
                    Border_Color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Cell_Color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Borders_Colors = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Boards", x => x.Boards_Id);
                    table.ForeignKey(
                        name: "FK_Boards_Users_Users_Id",
                        column: x => x.Users_Id,
                        principalTable: "Users",
                        principalColumn: "Users_Id");
                });

            migrationBuilder.CreateTable(
                name: "Decks",
                columns: table => new
                {
                    Decks_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Deck_Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Users_Id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Decks", x => x.Decks_Id);
                    table.ForeignKey(
                        name: "FK_Decks_Users_Users_Id",
                        column: x => x.Users_Id,
                        principalTable: "Users",
                        principalColumn: "Users_Id");
                });

            migrationBuilder.CreateTable(
                name: "Phases",
                columns: table => new
                {
                    Phases_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Decks_Id = table.Column<int>(type: "integer", nullable: false),
                    Phase_Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phases", x => x.Phases_Id);
                    table.ForeignKey(
                        name: "FK_Phases_Decks_Decks_Id",
                        column: x => x.Decks_Id,
                        principalTable: "Decks",
                        principalColumn: "Decks_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Processes",
                columns: table => new
                {
                    Processes_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Processes_Desc = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    Processes_Long_Desc = table.Column<string>(type: "TEXT", nullable: false),
                    Processes_Color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Processes_Weight = table.Column<double>(type: "double precision", nullable: false),
                    Decks_Id = table.Column<int>(type: "integer", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    Cards_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Decks_Id = table.Column<int>(type: "integer", nullable: false),
                    Phases_Id = table.Column<int>(type: "integer", nullable: true),
                    Card_Id = table.Column<int>(type: "integer", nullable: false)
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
                        name: "FK_Cards_Phases_Phases_Id",
                        column: x => x.Phases_Id,
                        principalTable: "Phases",
                        principalColumn: "Phases_Id");
                });

            migrationBuilder.CreateTable(
                name: "GameEvents",
                columns: table => new
                {
                    Games_Events_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Decks_Id = table.Column<int>(type: "integer", nullable: true),
                    Phases_Id = table.Column<int>(type: "integer", nullable: true),
                    Events_Short_Desc = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Events_Long_Desc = table.Column<string>(type: "TEXT", nullable: false),
                    Turns_Time = table.Column<int>(type: "integer", nullable: false),
                    Decisions_Costs_Bits_Weights = table.Column<double>(type: "double precision", nullable: true),
                    Decisions_Costs_PD_Weights = table.Column<double>(type: "double precision", nullable: true),
                    Hardwares_Costs_Bits_Weights = table.Column<double>(type: "double precision", nullable: true),
                    Hardwares_Costs_PD_Weights = table.Column<double>(type: "double precision", nullable: true),
                    Softwares_Costs_Bits_Weights = table.Column<double>(type: "double precision", nullable: true),
                    Softwares_Costs_PD_Weights = table.Column<double>(type: "double precision", nullable: true),
                    Boosters_X = table.Column<double>(type: "double precision", nullable: true),
                    Boosters_Y = table.Column<double>(type: "double precision", nullable: true)
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
                        name: "FK_GameEvents_Phases_Phases_Id",
                        column: x => x.Phases_Id,
                        principalTable: "Phases",
                        principalColumn: "Phases_Id");
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Games_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Games_Desc = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Teams_Boards_Id = table.Column<int>(type: "integer", nullable: false),
                    Rivals_Boards_Id = table.Column<int>(type: "integer", nullable: false),
                    Decks_Id = table.Column<int>(type: "integer", nullable: false),
                    Phases_Id = table.Column<int>(type: "integer", nullable: true),
                    Users_Id = table.Column<int>(type: "integer", nullable: false),
                    Game_Status = table.Column<int>(type: "integer", nullable: true),
                    Is_Online = table.Column<bool>(type: "boolean", nullable: false)
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
                        name: "FK_Games_Phases_Phases_Id",
                        column: x => x.Phases_Id,
                        principalTable: "Phases",
                        principalColumn: "Phases_Id");
                    table.ForeignKey(
                        name: "FK_Games_Users_Users_Id",
                        column: x => x.Users_Id,
                        principalTable: "Users",
                        principalColumn: "Users_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CardWeights",
                columns: table => new
                {
                    Cards_Weights_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Cards_Id = table.Column<int>(type: "integer", nullable: false),
                    Processes_Id = table.Column<int>(type: "integer", nullable: false),
                    Weights_X = table.Column<int>(type: "integer", nullable: false),
                    Weights_Y = table.Column<int>(type: "integer", nullable: false),
                    Booster_X = table.Column<double>(type: "double precision", nullable: false),
                    Booster_Y = table.Column<double>(type: "double precision", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "Decisions",
                columns: table => new
                {
                    Decisions_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Cards_Id = table.Column<int>(type: "integer", nullable: false),
                    Decisions_Short_Desc = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Decisions_Long_Desc = table.Column<string>(type: "TEXT", nullable: false),
                    Decisions_Cost_Bits = table.Column<double>(type: "double precision", nullable: false),
                    Decisions_Cost_Bits_Weight = table.Column<double>(type: "double precision", nullable: false),
                    Decisions_Cost_PD = table.Column<double>(type: "double precision", nullable: true),
                    Decisions_Cost_PD_Weight = table.Column<double>(type: "double precision", nullable: true),
                    Decisions_Reward_Bits = table.Column<double>(type: "double precision", nullable: true),
                    Decisions_Reward_Bits_Weight = table.Column<double>(type: "double precision", nullable: true),
                    Decisions_Reward_PD = table.Column<double>(type: "double precision", nullable: true),
                    Decisions_Reward_PD_Weight = table.Column<double>(type: "double precision", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    Feedbacks_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Cards_Id = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<bool>(type: "boolean", nullable: false),
                    Feedbacks_Long_Description = table.Column<string>(type: "TEXT", nullable: false),
                    Feedbacks_PDF = table.Column<byte[]>(type: "bytea", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "Hardwares",
                columns: table => new
                {
                    Hardwares_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Cards_Id = table.Column<int>(type: "integer", nullable: false),
                    Hardwares_Short_Desc = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Hardwares_Long_Desc = table.Column<string>(type: "TEXT", nullable: false),
                    Hardwares_Cost_Bits = table.Column<double>(type: "double precision", nullable: false),
                    Hardwares_Cost_Bits_Weight = table.Column<double>(type: "double precision", nullable: false),
                    Hardwares_Cost_PD = table.Column<double>(type: "double precision", nullable: true),
                    Hardwares_Cost_PD_Weight = table.Column<double>(type: "double precision", nullable: true),
                    Hardwares_Reward_Bits = table.Column<double>(type: "double precision", nullable: true),
                    Hardwares_Reward_Bits_Weight = table.Column<double>(type: "double precision", nullable: true),
                    Hardwares_Reward_PD = table.Column<double>(type: "double precision", nullable: true),
                    Hardwares_Reward_PD_Weight = table.Column<double>(type: "double precision", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "Softwares",
                columns: table => new
                {
                    Softwares_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Cards_Id = table.Column<int>(type: "integer", nullable: false),
                    Softwares_Short_Desc = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Softwares_Long_Desc = table.Column<string>(type: "TEXT", nullable: false),
                    Softwares_Cost_Bits = table.Column<double>(type: "double precision", nullable: false),
                    Softwares_Cost_Bits_Weight = table.Column<double>(type: "double precision", nullable: false),
                    Softwares_Cost_PD = table.Column<double>(type: "double precision", nullable: true),
                    Softwares_Cost_PD_Weight = table.Column<double>(type: "double precision", nullable: true),
                    Softwares_Reward_Bits = table.Column<double>(type: "double precision", nullable: true),
                    Softwares_Reward_Bits_Weight = table.Column<double>(type: "double precision", nullable: true),
                    Softwares_Reward_PD = table.Column<double>(type: "double precision", nullable: true),
                    Softwares_Reward_PD_Weight = table.Column<double>(type: "double precision", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Teams_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Games_Id = table.Column<int>(type: "integer", nullable: false),
                    Teams_Color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Teams_Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Teams_Bud = table.Column<double>(type: "double precision", nullable: false),
                    Teams_PD = table.Column<double>(type: "double precision", nullable: true),
                    Teams_Token = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: true),
                    Games_Events_Id = table.Column<int>(type: "integer", nullable: true),
                    Turns_Left = table.Column<int>(type: "integer", nullable: true),
                    Is_Independent = table.Column<bool>(type: "boolean", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "CardEnablers",
                columns: table => new
                {
                    Cards_Enablers_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Cards_Id = table.Column<int>(type: "integer", nullable: false),
                    Enablers_Id = table.Column<int>(type: "integer", nullable: true),
                    Cards_Enablers_Description = table.Column<string>(type: "text", nullable: false),
                    Games_Id = table.Column<int>(type: "integer", nullable: true),
                    Teams_Id = table.Column<int>(type: "integer", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "GameProcesses",
                columns: table => new
                {
                    Games_Processes_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Processes_Id = table.Column<int>(type: "integer", nullable: false),
                    Games_Id = table.Column<int>(type: "integer", nullable: false),
                    Teams_Id = table.Column<int>(type: "integer", nullable: false),
                    Games_Processes_Weights = table.Column<int>(type: "integer", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "GameLogs",
                columns: table => new
                {
                    Games_Logs_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Teams_Id = table.Column<int>(type: "integer", nullable: false),
                    Games_Id = table.Column<int>(type: "integer", nullable: false),
                    Games_Events_Id = table.Column<int>(type: "integer", nullable: true),
                    Cards_Id = table.Column<int>(type: "integer", nullable: true),
                    Boards_Id = table.Column<int>(type: "integer", nullable: true),
                    Feedbacks_Id = table.Column<int>(type: "integer", nullable: true),
                    EnablerFeedbacks_Id = table.Column<int>(type: "integer", nullable: true),
                    Costs = table.Column<double>(type: "double precision", nullable: true),
                    Status = table.Column<bool>(type: "boolean", nullable: true),
                    Is_Approved = table.Column<bool>(type: "boolean", nullable: true),
                    Booster_X = table.Column<double>(type: "double precision", nullable: true),
                    Booster_Y = table.Column<double>(type: "double precision", nullable: true)
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
                        name: "FK_GameLogs_CardEnablers_EnablerFeedbacks_Id",
                        column: x => x.EnablerFeedbacks_Id,
                        principalTable: "CardEnablers",
                        principalColumn: "Cards_Enablers_Id",
                        onDelete: ReferentialAction.SetNull);
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
                        principalColumn: "Teams_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameBoards",
                columns: table => new
                {
                    Games_Boards_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Teams_Id = table.Column<int>(type: "integer", nullable: false),
                    Games_Id = table.Column<int>(type: "integer", nullable: false),
                    Games_Processes_Id = table.Column<int>(type: "integer", nullable: true),
                    Poz_X = table.Column<double>(type: "double precision", nullable: false),
                    Poz_Y = table.Column<double>(type: "double precision", nullable: false),
                    Boards_Id = table.Column<int>(type: "integer", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "GameLogSpecs",
                columns: table => new
                {
                    Games_Logs_Specs_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Games_Logs_Id = table.Column<int>(type: "integer", nullable: false),
                    Games_Processes_Id = table.Column<int>(type: "integer", nullable: true),
                    Moves_X = table.Column<int>(type: "integer", nullable: false),
                    Moves_Y = table.Column<int>(type: "integer", nullable: false)
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
                });

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
                name: "IX_Cards_Phases_Id",
                table: "Cards",
                column: "Phases_Id");

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
                name: "IX_GameEvents_Phases_Id",
                table: "GameEvents",
                column: "Phases_Id");

            migrationBuilder.CreateIndex(
                name: "IX_GameLogs_Boards_Id",
                table: "GameLogs",
                column: "Boards_Id");

            migrationBuilder.CreateIndex(
                name: "IX_GameLogs_Cards_Id",
                table: "GameLogs",
                column: "Cards_Id");

            migrationBuilder.CreateIndex(
                name: "IX_GameLogs_EnablerFeedbacks_Id",
                table: "GameLogs",
                column: "EnablerFeedbacks_Id");

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
                name: "IX_Games_Phases_Id",
                table: "Games",
                column: "Phases_Id");

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
                name: "IX_Phases_Decks_Id",
                table: "Phases",
                column: "Decks_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Processes_Decks_Id",
                table: "Processes",
                column: "Decks_Id");

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
                name: "CardEnablers");

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
                name: "Phases");

            migrationBuilder.DropTable(
                name: "Decks");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
