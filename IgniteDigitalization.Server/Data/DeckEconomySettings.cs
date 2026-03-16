namespace backend.Data
{
    /// <summary>
    /// Przechowuje zasady ekonomii gry przypisane do konkretnego Szkolenia (Deck).
    /// Zasady opisują budżety Mapy 1 i Mapy 2 oraz mnożnik przygotowania (faza PRE → Rynkowa).
    /// </summary>
    public class DeckEconomySettings
    {
        public int DeckEconomySettings_Id { get; set; }

        /// <summary>Klucz obcy do Szkolenia (Deck)</summary>
        public int Decks_Id { get; set; }
        public Deck? Deck { get; set; }

        // --- MAPA 1: Budżet startowy ---

        /// <summary>Startowy budżet BITS dla drużyn w Mapie 1 (domyślnie 40)</summary>
        public double Map1_Starting_Budget { get; set; } = 40;

        /// <summary>Łączny koszt kart obowiązkowych (Enablers + wejście na rynek) w Mapie 1 (domyślnie 9)</summary>
        public double Map1_Mandatory_Cards_Cost { get; set; } = 9;

        /// <summary>Minimalna docelowa liczba kart do zagrania w Mapie 1 (domyślnie 14)</summary>
        public int Map1_Target_Cards_Min { get; set; } = 14;

        /// <summary>Maksymalna docelowa liczba kart do zagrania w Mapie 1 (domyślnie 16)</summary>
        public int Map1_Target_Cards_Max { get; set; } = 16;

        // --- MAPA 2: Budżet rynkowy ---

        /// <summary>Budżet bazowy dla Mapy 2 (bez premii przygotowania) (domyślnie 32)</summary>
        public double Map2_Base_Budget { get; set; } = 32;

        /// <summary>Maksymalna premia przygotowania w BITS (przy 22 kartach PRE) (domyślnie 12)</summary>
        public double Map2_Prep_Bonus_Max_Bits { get; set; } = 12;

        /// <summary>Łączna liczba kart PRE w talii – mianownik formuły premii (domyślnie 22)</summary>
        public int Map2_Prep_Cards_Total_Count { get; set; } = 22;

        /// <summary>Minimalna docelowa liczba kart do zagrania w Mapie 2 (domyślnie 11)</summary>
        public int Map2_Target_Cards_Min { get; set; } = 11;

        /// <summary>Maksymalna docelowa liczba kart do zagrania w Mapie 2 (domyślnie 13)</summary>
        public int Map2_Target_Cards_Max { get; set; } = 13;

        // --- Mnożnik przygotowania ---

        /// <summary>
        /// Maksymalny mnożnik przygotowania (domyślnie 2.0).
        /// Formuła: MIN(PrepMultiplier_Max; 1 + cardsPlayedPhase1 / Map2_Prep_Cards_Total_Count)
        /// Mnożnik jest stosowany do wag (Weights) kart na planszy procesów przy wejściu w Mapę 2.
        /// </summary>
        public double PrepMultiplier_Max { get; set; } = 2.0;
    }
}
