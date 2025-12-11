using System.ComponentModel.DataAnnotations;

namespace backend.Data
{
    public class Team
    {
        public int Teams_Id { get; set; }
        public int Games_Id { get; set; }
        public Game Games { get; set; } = null!;
        [MaxLength(7)]
        public string Teams_Color { get; set; } = string.Empty;
        [MaxLength(50)]
        public string Teams_Name { get; set; } = string.Empty;
        public double Teams_Bud { get; set; }
        public double? Teams_PD { get; set; }
        [MaxLength(6)]
        public string? Teams_Token { get; set; } = string.Empty;
        public int? Games_Events_Id { get; set; }
        public GameEvent? Games_Events { get; set; }
        public int? Turns_Left { get; set; }
        public bool Is_Independent { get; set; }

        public virtual ICollection<GameProcess> Game_Processes { get; set; } = new List<GameProcess>();
    }
}