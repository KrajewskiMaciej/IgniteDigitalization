
namespace DigitalWars.Server.Dtos
{
    public class TeamTokenInfo
    {
        public string Teams_Token { get; set; } = string.Empty;
        public int Teams_Id { get; set; }
        public int Games_Id { get; set; }
        public string Teams_Name { get; set; } = string.Empty;
        public string Teams_Color { get; set; } = string.Empty;
        public int Is_Online { get; set; }
        public int Is_Independent { get; set; }
        public int Teams_Boards_Id { get; set; }
        public int Rivals_Boards_Id { get; set; }
        public int Decks_Id { get; set; }
        public int? Modules_Id { get; set; }

    }

    public class TeamManagementDto
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public double TeamBud { get; set; }
        public string TeamColor { get; set; } = string.Empty;
        public int BoardId { get; set; }
        public int? DeckId { get; set; }
        public string? TeamToken { get; set; } = string.Empty;
    }

    public class TeamLoginRequestDto
    {
        public string Token { get; set; } = string.Empty;
    }
}