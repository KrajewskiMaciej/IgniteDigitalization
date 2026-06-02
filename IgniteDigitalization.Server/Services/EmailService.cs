using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using backend.Server.Settings;

namespace backend.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendConfirmationEmailAsync(string userEmail, string confirmationToken, DateTime? dateTime, string? baseUrl = null);
        Task SendPasswordResetEmailAsync(string userEmail, string resetToken, DateTime? dateTime);
        Task SendBugReportEmailsAsync(string reporterEmail, string description, string language);
    }

    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public bool EnableSSL { get; set; }
        public string SenderName { get; set; } = string.Empty;
        private string? _senderEmail;

        public string SenderEmail
        {
            get => !string.IsNullOrEmpty(_senderEmail) ? _senderEmail : SmtpUsername;
            set => _senderEmail = value;
        }

        public string BugReportEmailAddress { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonIgnore]
        public IEnumerable<string> BugReportEmailAddressesNormalized =>
            BugReportEmailAddresses ?? (string.IsNullOrWhiteSpace(BugReportEmailAddress)
                ? []
                : [BugReportEmailAddress]);

        public List<string>? BugReportEmailAddresses { get; set; }
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;
        private readonly FrontendSettings _frontendSettings;

        // Poprawny konstruktor
        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger, IConfiguration configuration, IOptions<FrontendSettings> frontendSettings)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
            _configuration = configuration;
            _frontendSettings = frontendSettings.Value;
        }

        // Poprawiona metoda, oznaczona jako `public async Task`
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                body = body.Replace("{{YEAR}}", DateTime.Now.Year.ToString());

                var fromAddress = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName);

                var mailMessage = new MailMessage
                {
                    From = fromAddress,
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(to);

                var alternateView = AlternateView.CreateAlternateViewFromString(body, null, "text/html");

                var svgLogoPath = Path.Combine(AppContext.BaseDirectory, "Templates", "ignite_logo.svg");
                var pngLogoPath = Path.Combine(AppContext.BaseDirectory, "Templates", "ignite_logo.png");

                string? resolvedLogoPath = File.Exists(svgLogoPath) ? svgLogoPath
                                         : File.Exists(pngLogoPath) ? pngLogoPath
                                         : null;
                string resolvedMimeType  = resolvedLogoPath?.EndsWith(".svg") == true ? "image/svg+xml" : "image/png";

                if (resolvedLogoPath != null)
                {
                    var logoResource = new LinkedResource(resolvedLogoPath, resolvedMimeType)
                    {
                        ContentId = "logo",
                        TransferEncoding = System.Net.Mime.TransferEncoding.Base64
                    };
                    alternateView.LinkedResources.Add(logoResource);
                }

                mailMessage.AlternateViews.Add(alternateView);

                using (var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort))
                {
                    client.Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
                    client.EnableSsl = _emailSettings.EnableSSL;

                    _logger.LogInformation("Wysyłanie e-maila do {To} z tematem {Subject}", to, subject);
                    await client.SendMailAsync(mailMessage); // 'await' działa, bo metoda jest 'async'
                    _logger.LogInformation("E-mail do {To} został pomyślnie wysłany.", to);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Wystąpił błąd podczas wysyłania e-maila do {To}", to);
                // Celowo nie rzucamy wyjątku dalej
            }
        }

        public async Task SendConfirmationEmailAsync(string userEmail, string confirmationToken, DateTime? expireDate, string? baseUrl = null)
        {
            // Krok 1: Zdefiniuj ścieżkę do szablonu
            var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "ConfirmationEmail.html");

            if (!File.Exists(templatePath))
            {
                _logger.LogError("Nie znaleziono szablonu e-maila w ścieżce: {TemplatePath}", templatePath);
                // Można rzucić wyjątek lub po prostu zakończyć, aby nie wysłać pustego maila
                return;
            }

            // Krok 2: Wczytaj całą zawartość pliku szablonu
            var emailBody = await File.ReadAllTextAsync(templatePath);

            // Krok 3: Przygotuj dynamiczne dane do wstawienia
            var effectiveBaseUrl = !string.IsNullOrEmpty(baseUrl) ? baseUrl : _frontendSettings.BaseUrl;
            var confirmationLink = $"{effectiveBaseUrl.TrimEnd('/')}/confirm/{confirmationToken}";
            string expireDateString = expireDate.HasValue ? $"{expireDate.Value:dd.MM.yyyy HH:mm}" : "brak daty";
            // Lub jeśli jesteś pewien, że wartość istnieje:
            if (expireDate.HasValue)
            {
                emailBody = emailBody.Replace("{{EXPIRE_DATE}}", $"{expireDate.Value:dd.MM.yyyy HH:mm}");
            }

            // Krok 4: Podmień placeholdery w szablonie
            emailBody = emailBody.Replace("{{CONFIRMATION_LINK}}", confirmationLink);
            emailBody = emailBody.Replace("{{EXPIRE_DATE}}", expireDateString);

            var subject = "Potwierdź swoją rejestrację w Digital Wars";

            // Krok 5: Wyślij e-mail z wczytaną i zmodyfikowaną treścią
            await SendEmailAsync(userEmail, subject, emailBody);
        }

        public async Task SendPasswordResetEmailAsync(string userEmail, string resetToken, DateTime? expireDate)
        {
            // Krok 1: Zdefiniuj ścieżkę do szablonu
            var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "ResetPasswordEmail.html");

            if (!File.Exists(templatePath))
            {
                _logger.LogError("Nie znaleziono szablonu e-maila w ścieżce: {TemplatePath}", templatePath);
                // Można rzucić wyjątek lub po prostu zakończyć, aby nie wysłać pustego maila
                return;
            }

            // Krok 2: Wczytaj całą zawartość pliku szablonu
            var emailBody = await File.ReadAllTextAsync(templatePath);

            // Krok 3: Przygotuj dynamiczne dane do wstawienia
            var resetLink = $"{_frontendSettings.BaseUrl}/resetPassword/{resetToken}";
            string expireDateString = expireDate.HasValue ? $"{expireDate.Value:dd.MM.yyyy HH:mm}" : "brak daty";
            // Lub jeśli jesteś pewien, że wartość istnieje:
            if (expireDate.HasValue)
            {
                emailBody = emailBody.Replace("{{EXPIRE_DATE}}", $"{expireDate.Value:dd.MM.yyyy HH:mm}");
            }

            // Krok 4: Podmień placeholdery w szablonie
            emailBody = emailBody.Replace("{{RESET_LINK}}", resetLink);
            emailBody = emailBody.Replace("{{EXPIRE_DATE}}", expireDateString);

            var subject = "Resetowanie hasła";

            // Krok 5: Wyślij e-mail z wczytaną i zmodyfikowaną treścią
            await SendEmailAsync(userEmail, subject, emailBody);
        }

        public async Task SendBugReportEmailsAsync(string reporterEmail, string description, string language)
        {
            // 1. Wyślij potwierdzenie do zgłaszającego (PL lub EN)
            var isPolish = string.IsNullOrEmpty(language) || language.ToLower() == "pl";
            var confirmTemplatePath = isPolish
                ? Path.Combine(AppContext.BaseDirectory, "Templates", "BugReportConfirmation.html")
                : Path.Combine(AppContext.BaseDirectory, "Templates", "en", "BugReportConfirmationEN.html");

            if (!File.Exists(confirmTemplatePath))
            {
                _logger.LogError("Nie znaleziono szablonu zgłoszenia błędu: {Path}", confirmTemplatePath);
            }
            else
            {
                var confirmBody = await File.ReadAllTextAsync(confirmTemplatePath);
                confirmBody = confirmBody
                    .Replace("{{REPORTER_EMAIL}}", reporterEmail)
                    .Replace("{{DESCRIPTION}}", description);

                var confirmSubject = isPolish
                    ? "Potwierdzenie zgłoszenia błędu – Ignite Digitalization"
                    : "Bug Report Confirmation – Ignite Digitalization";

                await SendEmailAsync(reporterEmail, confirmSubject, confirmBody);
            }

            // 2. Wyślij powiadomienie do itmserwis@itm.com.pl
            var notificationSubject = $"Nowe zgłoszenie błędu od {reporterEmail}";
            var notificationBody = BuildNotificationBody(reporterEmail, description);

            await SendEmailAsync(_emailSettings.SmtpUsername, notificationSubject, notificationBody);

            // 3. Wyślij powiadomienie na dodatkowe adresy (jeśli skonfigurowane)
            foreach (var address in _emailSettings.BugReportEmailAddressesNormalized)
            {
                if (!string.IsNullOrWhiteSpace(address))
                    await SendEmailAsync(address, notificationSubject, notificationBody);
            }
        }

        private static string BuildNotificationBody(string reporterEmail, string description)
        {
            return $"""
                <!DOCTYPE html>
                <html lang="pl">
                <head><meta charset="UTF-8"></head>
                <body style="margin:0;padding:0;font-family:Arial,Helvetica,sans-serif;background-color:#1e293b;">
                  <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="100%" style="background-color:#1e293b;">
                    <tr>
                      <td align="center" style="padding:20px 0;">
                        <table role="presentation" cellspacing="0" cellpadding="0" border="0" style="width:100%;max-width:600px;background-color:#334155;border-radius:8px;overflow:hidden;">
                          <tr>
                            <td style="background:linear-gradient(135deg,#a78bfa 0%,#8b5cf6 100%);padding:20px;text-align:center;">
                              <h2 style="color:#ffffff;margin:0;font-size:20px;">Nowe zgłoszenie błędu</h2>
                            </td>
                          </tr>
                          <tr>
                            <td style="padding:30px;">
                              <p style="color:#94a3b8;font-size:14px;margin:0 0 8px 0;">Zgłoszono przez:</p>
                              <p style="color:#a78bfa;font-size:16px;font-weight:bold;margin:0 0 24px 0;">{reporterEmail}</p>
                              <p style="color:#94a3b8;font-size:14px;margin:0 0 8px 0;">Opis błędu:</p>
                              <div style="background-color:#1e293b;border-radius:8px;border-left:4px solid #8b5cf6;padding:16px;">
                                <p style="color:#e2e8f0;font-size:15px;line-height:1.6;margin:0;white-space:pre-wrap;">{description}</p>
                              </div>
                            </td>
                          </tr>
                          <tr>
                            <td style="padding:20px;text-align:center;background-color:#1e293b;border-top:1px solid #475569;">
                              <p style="margin:0 0 8px 0;font-size:12px;color:#64748b;font-style:italic;">Wiadomość wygenerowana automatycznie — prosimy nie odpowiadać na ten e-mail.</p>
                              <p style="margin:0;font-size:12px;color:#64748b;">© {DateTime.Now.Year} ITM Software House. Wszelkie prawa zastrzeżone</p>
                            </td>
                          </tr>
                        </table>
                      </td>
                    </tr>
                  </table>
                </body>
                </html>
                """;
        }

    }
}
