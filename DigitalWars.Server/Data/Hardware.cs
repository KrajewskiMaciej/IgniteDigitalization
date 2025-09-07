using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data
{
    public class Hardware
    {
        public int Hardwares_Id { get; set; }
        public int Cards_Id { get; set; }
        public Card Cards { get; set; } = null!;
        [MaxLength(100)]
        public string Hardwares_Short_Desc { get; set; } = string.Empty;
        [Column(TypeName = "TEXT")]
        public string Hardwares_Long_Desc { get; set; } = string.Empty;
        public double Hardwares_Cost_Bits { get; set; }
        public double Hardwares_Cost_Bits_Weight { get; set; }
        public double? Hardwares_Cost_PD { get; set; }
        public double? Hardwares_Cost_PD_Weight { get; set; }
        public double? Hardwares_Reward_Bits { get; set; }
        public double? Hardwares_Reward_Bits_Weight { get; set; }
        public double? Hardwares_Reward_PD { get; set; }
        public double? Hardwares_Reward_PD_Weight { get; set; }
    }
}