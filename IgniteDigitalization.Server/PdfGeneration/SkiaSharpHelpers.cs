using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SkiaSharp;

namespace backend.PdfGeneration;

/// <summary>
/// Integracja SkiaSharp z QuestPDF zgodnie z:
/// https://www.questpdf.com/api-reference/skiasharp-integration.html
/// </summary>
public static class SkiaSharpHelpers
{
    /// <summary>
    /// Renderuje zawartość SkiaSharp jako SVG (wektory – zalecane).
    /// </summary>
    public static void SkiaSharpSvgCanvas(this IContainer container, Action<SKCanvas, Size> drawOnCanvas)
    {
        container.Svg(size =>
        {
            using var stream = new MemoryStream();

            using (var canvas = SKSvgCanvas.Create(new SKRect(0, 0, size.Width, size.Height), stream))
                drawOnCanvas(canvas, size);

            return Encoding.UTF8.GetString(stream.ToArray());
        });
    }
}
