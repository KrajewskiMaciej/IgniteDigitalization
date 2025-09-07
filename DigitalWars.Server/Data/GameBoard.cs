namespace backend.Data
{
    public class GameBoard
    {
        public int Games_Boards_Id { get; set; }
        public int Teams_Id { get; set; }
        public Team Teams { get; set; } = null!;
        public int Games_Id { get; set; }
        public Game Games { get; set; } = null!;
        public int? Games_Processes_Id { get; set; }
        public GameProcess? Games_Processes { get; set; }
        public double Poz_X { get; set; }
        public double Poz_Y { get; set; }
        public int Boards_Id { get; set; }
        public Board Boards { get; set; } = null!;
    }
}