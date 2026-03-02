using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using QRCoder;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace backend.PdfGeneration
{
    public class CardPdfModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Application { get; set; } = string.Empty;
        public string CardType { get; set; } = "Unknown";
        public double Cost { get; set; }
        public int PhaseId { get; set; }
        public string PhaseName { get; set; } = string.Empty;
    }

    public class CardsDocument : IDocument
    {
        private readonly List<CardPdfModel> _cards;
        private readonly ILogger<CardsDocument> _logger;

        private const string FontClother = "Clother";

        // Phase colours
        private const string ColorNavy   = "#1B2A4A";
        private const string ColorYellow = "#E8C040";
        private const string ColorPurple = "#6B3FA0";

        public CardsDocument(List<CardPdfModel> cards, ILogger<CardsDocument> logger)
        {
            _cards = cards;
            _logger = logger;

            var clotherPath = Path.Combine(AppContext.BaseDirectory, "Templates", "Clother.ttf");
            if (File.Exists(clotherPath))
            {
                try { QuestPDF.Drawing.FontManager.RegisterFont(File.OpenRead(clotherPath)); }
                catch { /* already registered */ }
            }
            else
            {
                _logger.LogWarning("[CardsDocument_Constructor] Nie znaleziono czcionki Clother.ttf w {Path}.", clotherPath);
            }

            _logger.LogInformation("[CardsDocument_Constructor] Zainicjalizowano dokument z {Count} kartami.", _cards.Count);
        }

        /// <summary>
        /// Returns 1/2/3 based on the sorted position of PhaseId within the document's card set.
        /// The lowest PhaseId = phase 1, next = 2, highest = 3.
        /// </summary>
        private int GetPhaseOrder(int phaseId)
        {
            var sorted = _cards
                .Select(c => c.PhaseId)
                .Distinct()
                .OrderBy(id => id)
                .ToList();
            var idx = sorted.IndexOf(phaseId);
            return idx >= 0 ? idx + 1 : 0;
        }

        private static string GetCircleColor(int phaseOrder) => phaseOrder switch
        {
            1 => ColorNavy,
            2 => ColorYellow,
            3 => ColorPurple,
            _ => ColorNavy
        };

        private static string GetTitleColor(int phaseOrder) =>
            phaseOrder == 2 ? ColorYellow : Colors.Black;

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            _logger.LogInformation("[CardsDocument_Compose] Rozpoczynam kompozycję dokumentu. Liczba kart: {Count}.", _cards.Count);

            // Pierwsza strona: rewers wspólny dla wszystkich kart
            if (_cards.Count > 0)
            {
                var logoPath     = Path.Combine(AppContext.BaseDirectory, "Templates", "Zasob_14x.png");
                var logotypyPath = Path.Combine(AppContext.BaseDirectory, "Templates", "Zasob_24x.png");
                _logger.LogInformation("[CardsDocument_Compose] Generuję stronę rewersu. Logo: {L}, Logotypy: {Lg}.", File.Exists(logoPath), File.Exists(logotypyPath));

                container.Page(page =>
                {
                    page.Size(2481, 1182, Unit.Point);
                    page.Margin(0);

                    page.Background()
                        .Border(8)
                        .BorderColor("#3A3600");

                    page.Content()
                        .PaddingTop(412)
                        .AlignCenter()
                        .Width(600)
                        .Image(logoPath)
                        .FitWidth();

                    page.Footer()
                        .PaddingHorizontal(80)
                        .PaddingBottom(205)
                        .Height(100)
                        .AlignCenter()
                        .Image(logotypyPath)
                        .FitHeight();
                });
            }

            // Pozostałe karty jako normalne strony (awers)
            foreach (var card in _cards)
            {
                _logger.LogInformation("[CardsDocument_Compose] Generuję awers karty Id={Id}, Tytuł='{Title}'.", card.Id, card.Title);
                container.Page(page =>
                {
                    page.Size(2481, 1182, Unit.Point);
                    page.Margin(0);
                    page.Content().Element(c => ComposeCard(c, card));
                });
            }

            _logger.LogInformation("[CardsDocument_Compose] Kompozycja dokumentu zakończona.");
        }

        private void ComposeCard(IContainer container, CardPdfModel card)
        {
            _logger.LogInformation("[CardsDocument_ComposeCard] Komponuję kartę Id={Id}, Faza='{Phase}', Koszt={Cost}.", card.Id, card.PhaseName, card.Cost);

            var phaseOrder  = GetPhaseOrder(card.PhaseId);
            var titleColor  = GetTitleColor(phaseOrder);
            var circleColor = GetCircleColor(phaseOrder);

            // For phase 2 wrap title with ★ symbols
            var displayTitle = phaseOrder == 2
                ? $"\u2605 {card.Title} \u2605"
                : card.Title;

            container
                .Background(Colors.White)
                .Border(8)
                .BorderColor("#3A3600")
                .Row(row =>
                {
                    // ── LEFT: text content ──────────────────────────────────
                    row.RelativeItem(6)
                        .PaddingLeft(110)
                        .PaddingRight(60)
                        .PaddingVertical(90)
                        .Column(col =>
                        {
                            col.Spacing(36);

                            col.Item()
                                .DefaultTextStyle(s => s
                                    .FontFamily(FontClother)
                                    .FontColor(titleColor))
                                .Text(displayTitle)
                                .FontSize(62);

                            col.Item()
                                .DefaultTextStyle(s => s
                                    .FontFamily(FontClother)
                                    .FontColor("#222222")
                                    .FontSize(32)
                                    .LineHeight(1.55f))
                                .Text(card.Description)
                                .Justify();

                            if (!string.IsNullOrWhiteSpace(card.Application))
                            {
                                col.Item()
                                    .DefaultTextStyle(s => s
                                        .FontFamily(FontClother)
                                        .FontColor("#222222")
                                        .FontSize(32)
                                        .LineHeight(1.55f))
                                    .Text(text =>
                                    {
                                        text.Span("Zastosowanie: ").Bold();
                                        text.Span(card.Application);
                                    });
                            }
                        });

                    // ── RIGHT: circle + QR code ─────────────────────────────
                    row.RelativeItem(3)
                        .PaddingTop(50)
                        .PaddingBottom(40)
                        .PaddingHorizontal(20)
                        .Column(col =>
                        {
                            col.Spacing(28);

                            // Coloured circle with badge
                            col.Item()
                                .DefaultTextStyle(s => s
                                    .FontFamily(FontClother)
                                    .FontColor(Colors.White))
                                .AlignCenter()
                                .Width(480)
                                .Height(480)
                                .Svg(GenerateCircleSvg(card.Id, card.PhaseName, card.Cost, circleColor));

                            // QR code
                            col.Item()
                                .AlignCenter()
                                .Width(300)
                                .Height(300)
                                .Image(GenerateQrCode(card.Id));
                        });
                });
        }

        private static string GenerateCircleSvg(int id, string phase, double cost, string circleColor)
        {
            var idText = id < 10 ? $"0{id}" : id.ToString();
            var costText = cost == Math.Floor(cost) ? $"{(int)cost}$" : $"{cost:F1}$";
            var phaseText = (phase ?? string.Empty).ToUpper();

            // Font-size for phase label — reduce if long
            var phaseFontSize = phaseText.Length > 14 ? 22 : 26;

            return $@"<svg viewBox='0 0 500 500' xmlns='http://www.w3.org/2000/svg'>
  <!-- Main circle -->
  <circle cx='250' cy='265' r='230' fill='{circleColor}' />

  <!-- Card number -->
  <text x='250' y='258'
        font-family='Arial, sans-serif'
        font-size='170'
        font-weight='bold'
        fill='white'
        text-anchor='middle'
        dominant-baseline='middle'>{idText}</text>

  <!-- Phase label -->
  <text x='250' y='390'
        font-family='Arial, sans-serif'
        font-size='{phaseFontSize}'
        font-weight='bold'
        letter-spacing='3'
        fill='white'
        text-anchor='middle'
        dominant-baseline='middle'>{phaseText}</text>

  <!-- Gold cost badge -->
  <circle cx='420' cy='55' r='68' fill='#E8C040' />

  <!-- Cost value -->
  <text x='420' y='42'
        font-family='Arial, sans-serif'
        font-size='32'
        font-weight='bold'
        fill='white'
        text-anchor='middle'
        dominant-baseline='middle'>{costText}</text>

  <!-- KOSZT label -->
  <text x='420' y='76'
        font-family='Arial, sans-serif'
        font-size='15'
        font-weight='bold'
        letter-spacing='2'
        fill='white'
        text-anchor='middle'
        dominant-baseline='middle'>KOSZT</text>
</svg>";
        }

        private static byte[] GenerateQrCode(int cardId)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(cardId.ToString(), QRCodeGenerator.ECCLevel.M);
            using var pngCode = new PngByteQRCode(qrCodeData);
            var result = pngCode.GetGraphic(12);
            return result;
        }
    }
}