using System.Collections.Generic;


namespace backend.Dtos
{
    public class ProcessEditDto
    {
        public string Process_Desc { get; set; } = string.Empty;
        public string Process_Long_Desc { get; set; } = string.Empty;

        public string Process_Color { get; set; } = string.Empty;
    }

    public class ProcessCreateDto
    {
        public int Deck_Id { get; set; }
        public string Process_Desc { get; set; } = string.Empty;
        public string Process_Long_Desc { get; set; } = string.Empty;

        public string Process_Color { get; set; } = string.Empty;
        public float Process_Weight { get; set; }
    }
}