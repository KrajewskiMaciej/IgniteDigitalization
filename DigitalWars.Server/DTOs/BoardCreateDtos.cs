using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class BoardCreateDto
    {
        [Required(ErrorMessage = "Nazwa planszy jest wymagana.")]
        [StringLength(50, ErrorMessage = "Nazwa planszy nie może przekraczać 50 znaków.")]
        public string Name { get; set; } = string.Empty;

        public string? Labels_Up { get; set; }

        public string? Labels_Right { get; set; }

        [StringLength(50, ErrorMessage = "Opis dolny nie może przekraczać 50 znaków.")]
        public string? Description_Down { get; set; }

        [StringLength(50, ErrorMessage = "Opis lewy nie może przekraczać 50 znaków.")]
        public string? Description_Left { get; set; }

        [Range(1, 100, ErrorMessage = "Liczba wierszy musi być między 1 a 100.")]
        public int Rows { get; set; }

        [Range(1, 100, ErrorMessage = "Liczba kolumn musi być między 1 a 100.")]
        public int Cols { get; set; }

        [Required]
        [StringLength(7, ErrorMessage = "Kod koloru obramowania musi mieć 7 znaków (np. #RRGGBB).")]
        [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Nieprawidłowy format koloru heksadecymalnego.")]
        public string Border_Color { get; set; } = string.Empty;

        [Required]
        [StringLength(7, ErrorMessage = "Kod koloru komórki musi mieć 7 znaków (np. #RRGGBB).")]
        [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Nieprawidłowy format koloru heksadecymalnego.")]
        public string Cell_Color { get; set; } = string.Empty;

        public string? Borders_Colors { get; set; }
    }
}