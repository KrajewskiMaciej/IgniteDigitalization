namespace backend.DTOs
{
    public class TeamDto
    {
        public string Name { get; set; } = string.Empty;
        public string Colour { get; set; } = string.Empty;
        public bool IsAbleToMakeDecisions { get; set; }
    }

    public class CreateGameDto
    {
        public string GameName { get; set; } = string.Empty;

        /// <summary>
        /// ID planszy drużynowej. Jeśli null, zostanie użyta domyślna plansza Szkolenia.
        /// </summary>
        public int? BoardId { get; set; }

        /// <summary>
        /// ID planszy rywali. Jeśli null, zostanie użyta domyślna plansza Szkolenia.
        /// </summary>
        public int? RivalBoardId { get; set; }

        public int DeckId { get; set; }
        public bool GameMode { get; set; }
        public int NumberOfTeams { get; set; }

        /// <summary>
        /// Startowy budżet BITS. Jeśli 0, zostanie automatycznie pobrany z zasad ekonomii Szkolenia.
        /// </summary>
        public int StartBits { get; set; }

        public List<TeamDto> Teams { get; set; } = new List<TeamDto>();
    }

    public class GameListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string DeckName { get; set; } = string.Empty;
    }

    // --- Podgląd ekonomii dla aktywnej gry ---
    public class GameEconomyPreviewDto
    {
        public int GameId { get; set; }
        public List<TeamEconomyPreviewDto> Teams { get; set; } = new();
    }

    public class TeamEconomyPreviewDto
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public int CardsPlayedPhase1 { get; set; }
        public double PreparationMultiplier { get; set; }
        public double ExpectedMap2Budget { get; set; }
    }
}