using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace backend.Data
{
    public class Game
    {
        public int Games_Id { get; set; }
        [MaxLength(50)]
        public string Games_Desc { get; set; } = string.Empty;

        public int Teams_Boards_Id { get; set; }
        public Board Teams_Boards { get; set; } = null!;

        public int Rivals_Boards_Id { get; set; }
        public Board Rivals_Boards { get; set; } = null!;
        public int Decks_Id { get; set; }
        public Deck Decks { get; set; } = null!;
        public int? Phases_Id { get; set; }
        public Phase? Phases { get; set; }
        public int Users_Id { get; set; }
        public User Users { get; set; } = null!;

        public GameStatus? Game_Status { get; set; }

        public bool Is_Online { get; set; }


        public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
        public virtual ICollection<GameBoard> GameBoards { get; set; } = new List<GameBoard>();
        public virtual ICollection<GameLog> GameLogs { get; set; } = new List<GameLog>();
        public virtual ICollection<GameProcess> GameProcesses { get; set; } = new List<GameProcess>();
    }

    public enum GameStatus
    {
        During,
        Paused,
        End
    }
}