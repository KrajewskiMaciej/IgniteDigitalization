namespace backend.Data
{
    public class GameLogSpec
    {
        public int Games_Logs_Specs_Id { get; set; }
        public int Games_Logs_Id { get; set; }
        public GameLog Games_Logs { get; set; } = null!;
        public int? Games_Processes_Id { get; set; }
        public GameProcess? Games_Processes { get; set; }
        public int Moves_X { get; set; }
        public int Moves_Y { get; set; }
    }
}