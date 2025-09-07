using System.Collections.Generic;

namespace backend.DTOs
{
    // Używane do wyświetlania kart w interfejsie gracza
    public class UnifiedCardDto
    {
        public int Id { get; set; }
        public int DeckId { get; set; }
        public int DisplayOrder { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CardType { get; set; } = string.Empty;
        public double Cost { get; set; }
        public List<int> Enablers { get; set; } = new List<int>();
    }

    // Używane przy zagrywaniu karty
    public class CardDataDTO
    {
        public int GameId { get; set; }
        public int TeamId { get; set; }
        public int DeckId { get; set; }
        public int BoardId { get; set; }
        public double Cost { get; set; }
        public bool ForceExecution { get; set; } = false;
    }

    // Używane do pobierania logów
    public class LogDataRequest
    {
        public int GameId { get; set; }
        public int TeamId { get; set; }
    }

    // Używane w panelu admina
    public class TeamManagementDto
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public int TeamBud { get; set; }
        public int BoardId { get; set; }
        public int? DeckId { get; set; }
        public string TeamToken { get; set; } = string.Empty;
    }

    public class UpdateBudgetDto
    {
        public int NewBudget { get; set; }
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
    }

    public class CategorizedCardsDto
    {
        public List<UnifiedCardDto> DecisionCards { get; set; } = new List<UnifiedCardDto>();
        public List<UnifiedCardDto> ItemCards { get; set; } = new List<UnifiedCardDto>();
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
}