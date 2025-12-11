using System.ComponentModel.DataAnnotations;

namespace backend.Data
{
    public class Deck
    {
        public int Decks_Id { get; set; }
        [MaxLength(50)]
        public string Deck_Name { get; set; } = string.Empty;
        public int? Users_Id { get; set; }
        public User? User { get; set; }
        public virtual ICollection<Process> Processes { get; set; } = new HashSet<Process>();

    }
}