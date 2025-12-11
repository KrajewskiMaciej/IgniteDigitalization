using System.Collections.Generic;

namespace DigitalWars.Server.Dtos
{
    // --- NOWE DTOs DLA BRAKUJĄCYCH ENDPOINTÓW ---
    public class BoardConfigDto
    {
        public int BoardId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string[] LabelsUp { get; set; } = Array.Empty<string>();
        public string[] LabelsRight { get; set; } = Array.Empty<string>();
        public string DescriptionDown { get; set; } = string.Empty;
        public string DescriptionLeft { get; set; } = string.Empty;
        public int Rows { get; set; }
        public int Cols { get; set; }
        public string CellColor { get; set; } = string.Empty;
        public string BorderColor { get; set; } = string.Empty;
        public string[] BorderColors { get; set; } = Array.Empty<string>();
    }

    public class SessionDataDto
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string TeamColor { get; set; } = string.Empty;
        public double TeamBud { get; set; }
        public int DeckId { get; set; }
        public BoardConfigDto BoardConfig { get; set; } = new BoardConfigDto();
    }


    // --- ISTNIEJĄCE DTOs (BEZ ZMIAN) ---
    public class UnifiedCardDto
    {
        public int Id { get; set; }
        public int DeckId { get; set; }
        public int DisplayOrder { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Cost { get; set; }
        public List<int> Enablers { get; set; } = new List<int>();
    }

    public class CardDataDto
    {
        public int CardId { get; set; }
        public int GameId { get; set; }
        public int TeamId { get; set; }
        public int DeckId { get; set; }
        public int BoardId { get; set; }
        public double Cost { get; set; }
        public bool ForceExecution { get; set; } = false;
    }

    public class LogDataRequest
    {
        public int GameId { get; set; }
        public int TeamId { get; set; }
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

    public class UpdateBudgetDto
    {
        public double NewBudget { get; set; }
        public int GameId { get; set; }
        public int TeamId { get; set; }
    }

    public class CardInfoDto
    {
        public int CardId { get; set; }
        public string CardName { get; set; } = string.Empty;
    }

    public class UnlockCardDto
    {
        public int CardId { get; set; }
        public int TeamId { get; set; }
        public int GameId { get; set; }
    }

    public class CategorizedCardsDto
    {
        public List<UnifiedCardDto> DecisionCards { get; set; } = new List<UnifiedCardDto>();
        public List<UnifiedCardDto> HardwareCards { get; set; } = new List<UnifiedCardDto>();
        public List<UnifiedCardDto> SoftwareCards { get; set; } = new List<UnifiedCardDto>();
    }

    public class GameEventDto
    {
        public int EventId { get; set; }
        public string ShortDesc { get; set; } = string.Empty;
        public string LongDesc { get; set; } = string.Empty;
    }

    public class ApplyEventDto
    {
        public int EventId { get; set; }
    }

    public class PlayerHistoryRequestDto
    {
        public int GameId { get; set; }
        public int? TeamId { get; set; }
    }
}

