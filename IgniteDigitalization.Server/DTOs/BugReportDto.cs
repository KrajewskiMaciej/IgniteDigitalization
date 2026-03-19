using System.ComponentModel.DataAnnotations;

namespace backend.Dtos
{
    public class BugReportDto
    {
        [Required(ErrorMessage = "Adres e-mail jest wymagany.")]
        [EmailAddress(ErrorMessage = "Podano nieprawidłowy adres e-mail.")]
        public string ReporterEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Opis błędu jest wymagany.")]
        [MinLength(10, ErrorMessage = "Opis błędu musi zawierać co najmniej 10 znaków.")]
        public string Description { get; set; } = string.Empty;
    }
}
