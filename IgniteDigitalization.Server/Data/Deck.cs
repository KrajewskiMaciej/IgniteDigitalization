using System.ComponentModel.DataAnnotations;

namespace backend.Data
{
    /// <summary>
    /// Szkolenie (dawniej: Talia kart) – zawiera karty, procesy, fazy oraz zasady ekonomii.
    /// </summary>
    public class Deck
    {
        public int Decks_Id { get; set; }

        [MaxLength(50)]
        public string Deck_Name { get; set; } = string.Empty;

        public int? Users_Id { get; set; }
        public User? User { get; set; }

        /// <summary>Domyślna plansza drużynowa (Mapa procesów) przypisana do Szkolenia</summary>
        public int? Default_Teams_Boards_Id { get; set; }
        public Board? DefaultTeamsBoard { get; set; }

        /// <summary>Domyślna plansza rynkowa (Mapa rywalizacji) przypisana do Szkolenia</summary>
        public int? Default_Rivals_Boards_Id { get; set; }
        public Board? DefaultRivalsBoard { get; set; }

        public virtual ICollection<Process> Processes { get; set; } = new HashSet<Process>();

        /// <summary>Zasady ekonomii przypisane do Szkolenia (relacja 1:1)</summary>
        public DeckEconomySettings? EconomySettings { get; set; }
    }
}
