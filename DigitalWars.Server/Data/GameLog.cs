namespace backend.Data
{
    public class GameLog
    {
        public int Games_Logs_Id { get; set; }
        public DateTime Data { get; set; }
        public int Teams_Id { get; set; }
        public Team? Teams { get; set; } = null!;
        public int Games_Id { get; set; }
        public Game Games { get; set; } = null!;
        public int? Games_Events_Id { get; set; }
        public GameEvent? Games_Events { get; set; }
        public int? Cards_Id { get; set; }
        public Card? Cards { get; set; } = null!;
        public int? Boards_Id { get; set; }
        public Board? Boards { get; set; } = null!;
        public int? Feedbacks_Id { get; set; }
        public Feedback? Feedbacks { get; set; }
        public double? Costs { get; set; }
        public bool? Status { get; set; }
        public bool? Is_Approved { get; set; }
        public double? Booster_X { get; set; }
        public double? Booster_Y { get; set; }


        public virtual ICollection<GameLogSpec> GameLogSpecs { get; set; } = new HashSet<GameLogSpec>();
    }
}