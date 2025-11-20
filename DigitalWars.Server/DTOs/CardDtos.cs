namespace backend.Dtos
{
    // DTO dla karty decyzyjnej (w ręce gracza)
    public class DecisionCardDto
    {
        public int PlayerCardId { get; set; } // Unikalny ID karty w talii gracza
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CardType { get; set; } = string.Empty; // Np. "Decyzja", "Sprzęt", "Oprogramowanie"
        public double Cost { get; set; }
        // Możesz dodać więcej pól w zależności od tego, co jest potrzebne na froncie
    }

    // DTO dla karty przedmiotu (na stole gracza)
    public class ItemCardDto
    {
        public int PlayerCardId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string EffectDescription { get; set; } = string.Empty; // Opis efektu, jaki daje karta
        public bool IsActive { get; set; } = false; // Czy przedmiot jest obecnie aktywny
    }

    public class CardTypeDto
    {
        public int Cards_Id { get; set; }
        public int Card_Id { get; set; }
        public string CardType { get; set; } = string.Empty;
    }

    public class CheatsheetMapDto
    {
        public Dictionary<int, List<int>> EnablersMap { get; set; } = new Dictionary<int, List<int>>();
        public List<CardTypeDto> CardTypes { get; set; } = new List<CardTypeDto>();
    }

    public class UpdateCardDto
    {
        public int CardId { get; set; }
        public string ShortDesc { get; set; } = string.Empty;
        public string LongDesc { get; set; } = string.Empty;
    }

}