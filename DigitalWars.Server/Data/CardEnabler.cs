namespace backend.Data
{
    public class CardEnabler
    {
        public int Cards_Enablers_Id { get; set; }
        public int Cards_Id { get; set; }
        public Card Cards { get; set; } = null!;
        public int? Enablers_Id { get; set; }
        public Card? Enablers { get; set; }

        public int? Games_Id { get; set; }
        public Game? Games { get; set; }

        public int? Teams_Id { get; set; }
        public Team? Teams { get; set; }
    }
}