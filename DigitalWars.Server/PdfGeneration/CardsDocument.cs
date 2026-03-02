using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using QRCoder;
using System.Collections.Generic;
using System;

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
        public string Phase { get; set; } = string.Empty;
    }

    public class CardsDocument : IDocument
    {
        private readonly List<CardPdfModel> _cards;

        public CardsDocument(List<CardPdfModel> cards)
        {
            _cards = cards;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            foreach (var card in _cards)
            {
                container.Page(page =>
                {
                    page.Size(1676, 792, Unit.Point);
                    page.Margin(0);
                    page.Content().Element(c => ComposeCard(c, card));
                });
            }
        }

        private void ComposeCard(IContainer container, CardPdfModel card)
        {
            container
                .Background(Colors.White)
                .Row(row =>
                {
                    // ── LEFT: text content ──────────────────────────────────
                    row.RelativeItem(6)
                        .PaddingLeft(90)
                        .PaddingRight(50)
                        .PaddingVertical(90)
                        .Column(col =>
                        {
                            col.Spacing(28);

                            col.Item()
                                .DefaultTextStyle(s => s
                                    .FontFamily(Fonts.Arial)
                                    .FontColor(Colors.Black))
                                .Text(card.Title)
                                .FontSize(50)
                                .Bold();

                            col.Item()
                                .DefaultTextStyle(s => s
                                    .FontFamily(Fonts.Arial)
                                    .FontColor("#333333")
                                    .FontSize(28)
                                    .LineHeight(1.45f))
                                .Text(card.Description);

                            if (!string.IsNullOrWhiteSpace(card.Application))
                            {
                                col.Item()
                                    .DefaultTextStyle(s => s
                                        .FontFamily(Fonts.Arial)
                                        .FontColor("#333333")
                                        .FontSize(28)
                                        .LineHeight(1.45f))
                                    .Text(text =>
                                    {
                                        text.Span("Zastosowanie: ").Bold();
                                        text.Span(card.Application);
                                    });
                            }
                        });

                    // ── RIGHT: circle + QR code ─────────────────────────────
                    row.RelativeItem(4)
                        .PaddingTop(36)
                        .PaddingBottom(20)
                        .PaddingHorizontal(20)
                        .Column(col =>
                        {
                            col.Spacing(24);

                            // Dark circle with badge
                            col.Item()
                                .AlignCenter()
                                .Width(430)
                                .Height(430)
                                .Svg(GenerateCircleSvg(card.Id, card.Phase, card.Cost));

                            // QR code
                            col.Item()
                                .AlignCenter()
                                .Width(190)
                                .Height(190)
                                .Image(GenerateQrCode(card.Id));
                        });
                });
        }

        private static string GenerateCircleSvg(int id, string phase, double cost)
        {
            var idText = id < 10 ? $"0{id}" : id.ToString();
            var costText = cost == Math.Floor(cost) ? $"{(int)cost}$" : $"{cost:F1}$";
            var phaseText = (phase ?? string.Empty).ToUpper();

            // Font-size for phase label — reduce if long
            var phaseFontSize = phaseText.Length > 14 ? 17 : 20;

            return $@"<svg viewBox='0 0 500 500' xmlns='http://www.w3.org/2000/svg'>
  <!-- Main dark circle -->
  <circle cx='250' cy='265' r='222' fill='#1B2A4A' />

  <!-- Card number -->
  <text x='250' y='255'
        font-family='Arial, sans-serif'
        font-size='145'
        font-weight='bold'
        fill='white'
        text-anchor='middle'
        dominant-baseline='middle'>{idText}</text>

  <!-- Phase label -->
  <text x='250' y='370'
        font-family='Arial, sans-serif'
        font-size='{phaseFontSize}'
        font-weight='bold'
        letter-spacing='4'
        fill='white'
        text-anchor='middle'
        dominant-baseline='middle'>{phaseText}</text>

  <!-- Gold cost badge -->
  <circle cx='408' cy='68' r='62' fill='#E8C040' />

  <!-- Cost value -->
  <text x='408' y='55'
        font-family='Arial, sans-serif'
        font-size='28'
        font-weight='bold'
        fill='white'
        text-anchor='middle'
        dominant-baseline='middle'>{costText}</text>

  <!-- KOSZT label -->
  <text x='408' y='88'
        font-family='Arial, sans-serif'
        font-size='13'
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
            return pngCode.GetGraphic(12);
        }
    }
}