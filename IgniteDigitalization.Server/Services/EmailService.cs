using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using backend.Server.Settings;

namespace backend.Services
{
    public interface IEmailService
    {
        Task SendConfirmationEmailAsync(string userEmail, string confirmationToken, DateTime? dateTime, string? baseUrl = null);
        Task SendPasswordResetEmailAsync(string userEmail, string resetToken, DateTime? dateTime);
        Task SendBugReportEmailsAsync(string reporterEmail, string description, string language);
    }

    /// <summary>
    /// Konfiguracja adresatów zgłoszeń błędów. Po migracji na MessageService Ignite nie wysyła już
    /// SMTP bezpośrednio — poświadczenia SMTP żyją w MessageService. Tu zostają tylko adresy powiadomień:
    /// <see cref="NotifyAddress"/> (adres serwisowy) + opcjonalna lista dodatkowych adresów.
    /// </summary>
    public class EmailSettings
    {
        /// <summary>Adres serwisowy, na który trafia wewnętrzne powiadomienie o nowym zgłoszeniu błędu.</summary>
        public string NotifyAddress { get; set; } = string.Empty;

        public string BugReportEmailAddress { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonIgnore]
        public IEnumerable<string> BugReportEmailAddressesNormalized =>
            BugReportEmailAddresses ?? (string.IsNullOrWhiteSpace(BugReportEmailAddress)
                ? []
                : [BugReportEmailAddress]);

        public List<string>? BugReportEmailAddresses { get; set; }
    }

    /// <summary>
    /// Cienki klient HTTP do centralnego MessageService. Nie wysyła SMTP bezpośrednio — buduje dane
    /// (odbiorcy, szablon, placeholdery) i POST-uje do /api/Email/Send. Szablony i wygląd są własnością
    /// MessageService (aplikacja "Ignite"). Błędy są logowane i połykane (jak w poprzedniej implementacji).
    /// </summary>
    public class EmailService : IEmailService
    {
        private const string Application = "Ignite";

        private readonly HttpClient _httpClient;
        private readonly EmailSettings _emailSettings;
        private readonly FrontendSettings _frontendSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            HttpClient httpClient,
            IOptions<EmailSettings> emailSettings,
            IOptions<FrontendSettings> frontendSettings,
            ILogger<EmailService> logger)
        {
            _httpClient = httpClient;
            _emailSettings = emailSettings.Value;
            _frontendSettings = frontendSettings.Value;
            _logger = logger;
        }

        public async Task SendConfirmationEmailAsync(string userEmail, string confirmationToken, DateTime? expireDate, string? baseUrl = null)
        {
            var effectiveBaseUrl = !string.IsNullOrEmpty(baseUrl) ? baseUrl : _frontendSettings.BaseUrl;
            var confirmationLink = $"{effectiveBaseUrl.TrimEnd('/')}/confirm/{confirmationToken}";

            await PostSendAsync(
                template: "ConfirmationEmail",
                language: "pl",
                to: [userEmail],
                subject: "Potwierdź swoją rejestrację w Digital Wars",
                placeholders: new Dictionary<string, string>
                {
                    ["CONFIRMATION_LINK"] = confirmationLink,
                    ["EXPIRE_DATE"] = FormatExpire(expireDate)
                });
        }

        public async Task SendPasswordResetEmailAsync(string userEmail, string resetToken, DateTime? expireDate)
        {
            var resetLink = $"{_frontendSettings.BaseUrl.TrimEnd('/')}/resetPassword/{resetToken}";

            await PostSendAsync(
                template: "ResetPasswordEmail",
                language: "pl",
                to: [userEmail],
                subject: "Resetowanie hasła",
                placeholders: new Dictionary<string, string>
                {
                    ["RESET_LINK"] = resetLink,
                    ["EXPIRE_DATE"] = FormatExpire(expireDate)
                });
        }

        public async Task SendBugReportEmailsAsync(string reporterEmail, string description, string language)
        {
            var isPolish = string.IsNullOrEmpty(language) || language.ToLower() == "pl";

            // 1. Potwierdzenie do zgłaszającego (PL lub EN).
            await PostSendAsync(
                template: "BugReportConfirmation",
                language: isPolish ? "pl" : "en",
                to: [reporterEmail],
                subject: isPolish
                    ? "Potwierdzenie zgłoszenia błędu – Ignite Digitalization"
                    : "Bug Report Confirmation – Ignite Digitalization",
                placeholders: new Dictionary<string, string>
                {
                    ["REPORTER_EMAIL"] = reporterEmail,
                    ["DESCRIPTION"] = description
                });

            // 2. Powiadomienie wewnętrzne: adres serwisowy + dodatkowe adresy z konfiguracji (bez pustych).
            var notificationRecipients = new[] { _emailSettings.NotifyAddress }
                .Concat(_emailSettings.BugReportEmailAddressesNormalized)
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .ToList();

            await PostSendAsync(
                template: "BugReportNotification",
                language: "pl",
                to: notificationRecipients,
                subject: $"Nowe zgłoszenie błędu od {reporterEmail}",
                placeholders: new Dictionary<string, string>
                {
                    ["REPORTER_EMAIL"] = reporterEmail,
                    ["DESCRIPTION"] = description
                });
        }

        private static string FormatExpire(DateTime? expireDate) =>
            expireDate.HasValue ? $"{expireDate.Value:dd.MM.yyyy HH:mm}" : "brak daty";

        /// <summary>Wysyła żądanie do MessageService. Błędy loguje i połyka (nie przerywa flow aplikacji).</summary>
        private async Task PostSendAsync(string template, string language, List<string> to, string subject, Dictionary<string, string> placeholders)
        {
            if (_httpClient.BaseAddress is null)
            {
                _logger.LogWarning("[Ignite_EmailService] MessageService:BaseUrl nieustawione — pomijam wysyłkę. Template={Template}, To={To}", template, string.Join(",", to));
                return;
            }

            var payload = new
            {
                application = Application,
                template,
                language,
                to,
                subject,
                placeholders
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/Email/Send", payload);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("[Ignite_EmailService] Wysłano żądanie do MessageService. Template={Template}, To={To}", template, string.Join(",", to));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Ignite_EmailService] Błąd wysyłki przez MessageService. Template={Template}, To={To}", template, string.Join(",", to));
                // Celowo nie rzucamy dalej — zachowujemy dotychczasowe zachowanie (połykanie błędów maila).
            }
        }
    }
}
