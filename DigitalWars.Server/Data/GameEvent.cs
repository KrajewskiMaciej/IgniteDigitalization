using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data
{
    public class GameEvent
    {
        public int Games_Events_Id { get; set; }
        public int? Decks_Id { get; set; }
        public Deck Decks { get; set; } = null!;
        public int? Modules_Id { get; set; }
        public Module Modules { get; set; } = null!;
        [MaxLength(100)]
        public string Events_Short_Desc { get; set; } = string.Empty;
        [Column(TypeName = "TEXT")]
        public string Events_Long_Desc { get; set; } = string.Empty;
        public int Turns_Time { get; set; }
        public double? Decisions_Costs_Bits_Weights { get; set; }
        public double? Decisions_Costs_PD_Weights { get; set; }
        public double? Hardwares_Costs_Bits_Weights { get; set; }
        public double? Hardwares_Costs_PD_Weights { get; set; }
        public double? Softwares_Costs_Bits_Weights { get; set; }
        public double? Softwares_Costs_PD_Weights { get; set; }
        public double? Boosters_X { get; set; }
        public double? Boosters_Y { get; set; }

        public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
        public virtual ICollection<GameLog> GameLogs { get; set; } = new List<GameLog>();

    }
}