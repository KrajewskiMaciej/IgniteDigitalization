using backend.Dtos;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BugReportController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<BugReportController> _logger;

        public BugReportController(IEmailService emailService, ILogger<BugReportController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Przyjmuje zgłoszenie błędu. Dostępne bez autoryzacji.
        /// Query param: lang (pl/en), domyślnie pl.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SubmitBugReport(
            [FromBody] BugReportDto dto,
            [FromQuery] string lang = "pl")
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _emailService.SendBugReportEmailsAsync(dto.ReporterEmail, dto.Description, lang);
                _logger.LogInformation("Zgłoszenie błędu wysłane przez {Email}", dto.ReporterEmail);
                return Ok(new { success = true, message = "Zgłoszenie zostało wysłane." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas wysyłania zgłoszenia od {Email}", dto.ReporterEmail);
                return StatusCode(500, new { success = false, message = "Wystąpił błąd podczas wysyłania zgłoszenia." });
            }
        }
    }
}
