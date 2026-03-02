namespace backend.Dtos
{
    // --- Edycja Szkolenia (nazwa + domyślne plansze) ---
    public class EditDeckDto
    {
        public int Decks_Id { get; set; }
        public string Decks_Name { get; set; } = string.Empty;
        public int? Default_Teams_Boards_Id { get; set; }
        public int? Default_Rivals_Boards_Id { get; set; }
    }

    // --- Odpowiedź z listą Szkoleń ---
    public class DeckListItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int? DefaultTeamsBoardId { get; set; }
        public int? DefaultRivalsBoardId { get; set; }
        public string? DefaultTeamsBoardName { get; set; }
        public string? DefaultRivalsBoardName { get; set; }
    }

    // --- Zasady ekonomii Szkolenia (odczyt + zapis) ---
    public class DeckEconomySettingsDto
    {
        public int DeckEconomySettings_Id { get; set; }
        public int Decks_Id { get; set; }

        // Mapa 1
        public double Map1_Starting_Budget { get; set; } = 40;
        public double Map1_Mandatory_Cards_Cost { get; set; } = 9;
        public int Map1_Target_Cards_Min { get; set; } = 14;
        public int Map1_Target_Cards_Max { get; set; } = 16;

        // Mapa 2
        public double Map2_Base_Budget { get; set; } = 32;
        public double Map2_Prep_Bonus_Max_Bits { get; set; } = 12;
        public int Map2_Prep_Cards_Total_Count { get; set; } = 22;
        public int Map2_Target_Cards_Min { get; set; } = 11;
        public int Map2_Target_Cards_Max { get; set; } = 13;

        // Mnożnik przygotowania
        public double PrepMultiplier_Max { get; set; } = 2.0;
    }

    public class UpdateDeckEconomySettingsDto
    {
        // Mapa 1
        public double Map1_Starting_Budget { get; set; }
        public double Map1_Mandatory_Cards_Cost { get; set; }
        public int Map1_Target_Cards_Min { get; set; }
        public int Map1_Target_Cards_Max { get; set; }

        // Mapa 2
        public double Map2_Base_Budget { get; set; }
        public double Map2_Prep_Bonus_Max_Bits { get; set; }
        public int Map2_Prep_Cards_Total_Count { get; set; }
        public int Map2_Target_Cards_Min { get; set; }
        public int Map2_Target_Cards_Max { get; set; }

        // Mnożnik przygotowania
        public double PrepMultiplier_Max { get; set; }
    }
}