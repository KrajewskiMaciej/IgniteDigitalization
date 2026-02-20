using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data
{
    public class Process
    {
        public int Processes_Id { get; set; }
        [MaxLength(25)]
        public string Processes_Desc { get; set; } = string.Empty;
        [MaxLength(100)]
        public string Processes_Long_Desc { get; set; } = string.Empty;
        [MaxLength(7)]
        public string Processes_Color { get; set; } = string.Empty;
        public double Processes_Weight { get; set; }
        public int Decks_Id { get; set; }
        public Deck Decks { get; set; } = null!;
        public int? Modules_Id { get; set; }
        public Module? Modules { get; set; } = null!;
    }
}