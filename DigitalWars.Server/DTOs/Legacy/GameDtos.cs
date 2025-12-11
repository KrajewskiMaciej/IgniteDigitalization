namespace backend.DTOs
{
    public class ProcessDto
    {
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
    }

    public class TeamDto
    {
        public string Name { get; set; } = string.Empty;
        public string Colour { get; set; } = string.Empty;
        public bool IsAbleToMakeDecisions { get; set; }
    }

    public class CreateGameDto
    {
        public string GameName { get; set; } = string.Empty;
        public int BoardId { get; set; }
        public int RivalBoardId { get; set; }
        public int DeckId { get; set; }
        public bool GameMode { get; set; }
        public int NumberOfTeams { get; set; }
        public int StartBits { get; set; }
        public List<TeamDto> Teams { get; set; } = new List<TeamDto>();
        public List<ProcessDto> Processes { get; set; } = new List<ProcessDto>();
    }

    public class GameListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}