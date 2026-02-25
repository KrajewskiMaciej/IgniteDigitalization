using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data
{
    public class Card
    {
        public int Cards_Id { get; set; }
        public int Decks_Id { get; set; }
        public Deck? Deck { get; set; }
        public int? Phases_Id { get; set; }
        public Phase? Phase { get; set; }
        public int Card_Id { get; set; }
        public ICollection<CardEnabler> DecisionEnablers { get; set; } = new List<CardEnabler>();
        public ICollection<CardEnabler> DecisionEnablerOfThis { get; set; } = new List<CardEnabler>();
    }
}