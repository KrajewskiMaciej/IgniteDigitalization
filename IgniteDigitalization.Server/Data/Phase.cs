using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data
{
    public class Phase
    {
        public int Phases_Id { get; set; }
        public int Decks_Id { get; set; }
        public Deck? Deck { get; set; }
        public string Phase_Name { get; set; } = string.Empty;
    }
}