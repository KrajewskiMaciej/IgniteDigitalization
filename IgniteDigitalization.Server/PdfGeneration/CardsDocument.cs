using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using QRCoder;
using System.Collections.Generic;
using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace backend.PdfGeneration
{
    public class CardPdfModel
    {
        public int Id { get; set; }       // Card_Id — numer widoczny na karcie
        public int CardsId { get; set; }  // Cards_Id — unikalny identyfikator egzemplarza (do QR)
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Cost { get; set; }
        public int PhaseId { get; set; }
        public string PhaseName { get; set; } = string.Empty;
    }

    public class CardsDocument : IDocument
    {
        private readonly List<CardPdfModel> _cards;
        private readonly ILogger<CardsDocument> _logger;
        // phaseId -> (circleColor, textColor)  — kolejność: 1=przygotowawcza, 2=wejście, 3=rynkowa
        private readonly Dictionary<int, (string circle, string text)> _phaseColors;

        private const string FontInter = "Inter";
        private const string ColorDarkBlue = "#1B2333";
        private const string ColorYellow = "#FFD752";
        private const string ColorPurple = "#5B2D8E";

        private string GetCircleColor(int phaseId) =>
            _phaseColors.TryGetValue(phaseId, out var c) ? c.circle : ColorDarkBlue;

        private string GetNumberColor(int phaseId) =>
            _phaseColors.TryGetValue(phaseId, out var c) ? c.text : Colors.White;

        private const float CardWidth = 600;
        private const float CardHeight = 286;

        /// <param name="orderedPhaseIds">Identyfikatory faz w kolejności: [0]=przygotowawcza, [1]=wejście na rynek, [2]=rynkowa</param>
        public CardsDocument(List<CardPdfModel> cards, ILogger<CardsDocument> logger, IList<int> orderedPhaseIds)
        {
            _cards = cards;
            _logger = logger;

            var colors = new (string circle, string text)[] {
                (ColorDarkBlue, Colors.White),
                (ColorYellow,   ColorDarkBlue),
                (ColorPurple,   Colors.White),
            };
            _phaseColors = new Dictionary<int, (string, string)>();
            for (int i = 0; i < orderedPhaseIds.Count && i < colors.Length; i++)
                _phaseColors[orderedPhaseIds[i]] = colors[i];
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            // --- REWERS ---
            if (_cards.Count > 0)
            {
                var logoPath = Path.Combine(AppContext.BaseDirectory, "Templates", "Zasob_14x.png");
                var logotypyPath = Path.Combine(AppContext.BaseDirectory, "Templates", "Zasob_24x.png");

                container.Page(page =>
                {
                    page.Size(CardWidth, CardHeight, Unit.Point);
                    page.PageColor(Colors.White);
                    page.Content().Layers(layers =>
                    {
                        layers.PrimaryLayer().Width(CardWidth).Height(CardHeight).Container();

                        // Logo Główne (X: 225, Y: 80)
                        layers.Layer().TranslateX(220).TranslateY(100).Width(144).Image(logoPath);

                        // Logotypy Partnerów (X: 50, Y: 210) - Jawne wymiary gwarantują widoczność
                        if (File.Exists(logotypyPath))
                        {
                            layers.Layer().TranslateX(125).TranslateY(212).Width(332).Height(24)
                                .Image(logotypyPath).FitArea();
                        }
                    });
                });
            }

            // --- AWERSY ---
            foreach (var card in _cards)
            {
                container.Page(page =>
                {
                    page.Size(CardWidth, CardHeight, Unit.Point);
                    page.PageColor(Colors.White);
                    page.Content().Element(c => ComposeCard(c, card));
                });
            }
        }

        private void ComposeCard(IContainer container, CardPdfModel card)
        {
            var circleColor = GetCircleColor(card.PhaseId);
            // Wyznacz PhaseId odpowiadający pozycji 1 (wejście na rynek)
            var phase2Id = _phaseColors.Keys.OrderBy(k => k).Skip(1).FirstOrDefault();
            var isEntryPhase = card.PhaseId == phase2Id;

            container.Layers(layers =>
            {
                // Podkładka 0,0
                layers.PrimaryLayer().Width(CardWidth).Height(CardHeight).Container();

                // 1. TYTUŁ (X: 50, Y: 45)
                var titleColor = isEntryPhase ? circleColor : "#000000";
                layers.Layer().TranslateX(50).TranslateY(67).Width(370).Height(24)
                    .Text(card.Title).FontFamily(FontInter).FontSize(18).Black().FontColor(titleColor);

                // 2. OPIS (X: 50, Y: 90)
                layers.Layer().TranslateX(51).TranslateY(100).Width(340).Height(180)
                    .Text(card.Description).FontFamily(FontInter).FontSize(15).Light().LineHeight(1.15f).Justify();

                // --- SEKCJA KOŁA FAZY (Baza X: 435, Y: 42) ---

                // Okrąg fazy
                layers.Layer().TranslateX(435).TranslateY(42).Width(100).Height(100)
                    .Svg(GetCircleSvg(circleColor));

                // Numer karty (np. 01) - zawsze biały
                layers.Layer().TranslateX(451).TranslateY(60).Width(70)
                    .AlignCenter().Text(card.Id.ToString("D2"))
                    .FontFamily(FontInter).FontSize(48).Black().FontColor(Colors.White);

                // Nazwa fazy (np. PRZYGOTOWANIE) - zawsze biała
                layers.Layer().TranslateX(451).TranslateY(112).Width(70)
                    .AlignCenter().Text(card.PhaseName.ToUpper())
                    .FontFamily(FontInter).FontSize(6).Black().FontColor(Colors.White);

                // --- SEKCJA ŻÓŁTEGO KOŁA KOSZTU (tylko gdy NIE jest fazą wejście na rynek) ---
                if (!isEntryPhase)
                {
                    // Okrąg żółty
                    layers.Layer().TranslateX(503).TranslateY(19).Width(50).Height(50)
                        .Svg(GetCircleSvg(ColorYellow));

                    // Wartość kosztu (np. 2$) - Wycentrowana w żółtym kole
                    layers.Layer().TranslateX(513).TranslateY(27).Width(30)
                        .AlignCenter().Text($"{card.Cost}$")
                        .FontFamily(FontInter).FontSize(21).Black().FontColor(Colors.White);

                    // Słowo "KOSZT" - Wycentrowane pod wartością
                    layers.Layer().TranslateX(512).TranslateY(52).Width(32)
                        .AlignCenter().Text("KOSZT")
                        .FontFamily(FontInter).FontSize(6).Black().FontColor(Colors.White);
                }

                // --- KOD QR (X: 454, Y: 168) ---
                layers.Layer().TranslateX(435).TranslateY(168).Width(90).Height(90)
                    .Image(GenerateQrCode(card.CardsId));
            });
        }

        // Metoda generująca SVG gwarantuje IDEALNE KOŁO bez błędów kompilacji
        private string GetCircleSvg(string hexColor)
            => $@"<svg viewBox='0 0 100 100' xmlns='http://www.w3.org/2000/svg'><circle cx='50' cy='50' r='50' fill='{hexColor}' /></svg>";

        private static byte[] GenerateQrCode(int cardId)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(cardId.ToString(), QRCodeGenerator.ECCLevel.M);
            using var pngCode = new PngByteQRCode(qrCodeData);
            return pngCode.GetGraphic(15);
        }
    }
}