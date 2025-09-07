using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data
{
    public class Decision
    {
        public int Decisions_Id { get; set; }
        public int Cards_Id { get; set; }
        public Card Card { get; set; } = null!;
        [MaxLength(100)]
        public string Decisions_Short_Desc { get; set; } = string.Empty;
        [Column(TypeName = "TEXT")]
        public string Decisions_Long_Desc { get; set; } = string.Empty;
        public double Decisions_Cost_Bits { get; set; }
        public double Decisions_Cost_Bits_Weight { get; set; }
        public double? Decisions_Cost_PD { get; set; }
        public double? Decisions_Cost_PD_Weight { get; set; }
        public double? Decisions_Reward_Bits { get; set; }
        public double? Decisions_Reward_Bits_Weight { get; set; }
        public double? Decisions_Reward_PD { get; set; }
        public double? Decisions_Reward_PD_Weight { get; set; }
    }
}