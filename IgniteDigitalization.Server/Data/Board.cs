using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data
{
    public class Board
    {
        public int Boards_Id { get; set; }
        public int? Users_Id { get; set; }
        public User? User { get; set; }
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        [Column(TypeName = "TEXT")]
        public string Labels_Up { get; set; } = string.Empty;
        [Column(TypeName = "TEXT")]
        public string Labels_Right { get; set; } = string.Empty;
        [MaxLength(50)]
        public string Description_Down { get; set; } = string.Empty;
        [MaxLength(50)]
        public string Description_Left { get; set; } = string.Empty;
        public int Rows { get; set; }
        public int Cols { get; set; }
        [MaxLength(7)]
        public string Border_Color { get; set; } = string.Empty;
        [MaxLength(7)]
        public string Cell_Color { get; set; } = string.Empty;
        [Column(TypeName = "TEXT")]
        public string Borders_Colors { get; set; } = string.Empty;
        [Column(TypeName = "TEXT")]
        public string Cells_Descriptions { get; set; } = string.Empty;

        public virtual ICollection<Game> TeamGames { get; set; } = new List<Game>();
        public virtual ICollection<Game> RivalGames { get; set; } = new List<Game>();
    }
}
