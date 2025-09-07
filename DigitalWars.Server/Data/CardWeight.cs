namespace backend.Data
{
    public class CardWeight
    {
        public int Cards_Weights_Id { get; set; }
        public int Cards_Id { get; set; }
        public Card Cards { get; set; } = null!;
        public int Processes_Id { get; set; }
        public Process Processes { get; set; } = null!;
        public int Weights_X { get; set; }
        public int Weights_Y { get; set; }
        public double Booster_X { get; set; }
        public double Booster_Y { get; set; }
    }
}