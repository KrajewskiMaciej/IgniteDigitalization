using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using backend.Data;

namespace backend.PdfGeneration
{
    public class BoardsDocument : IDocument
    {
        private readonly List<Board> _boardsToRender;
        private readonly int _teamBoardId;
        private readonly int _rivalBoardId;

        // ── Stałe layoutu (nieskalowane) ─────────────────────────────────────
        // Kartezjańska: [DescLeft=14 | AxisLbl=12 | Num=11 | GRID | Num=11 | AxisLbl=12]
        //               [TopLbl=11  | Letters=10  | GRID | Letters=10 | BotLbl=11 | DescDown=14]
        private const float C_Cell = 30f;
        private const float C_DescW = 14f;
        private const float C_AxisW = 12f;
        private const float C_NumW = 11f;
        private const float C_LetH = 10f;
        private const float C_TopH = 11f;
        private const float C_BotH = 11f;
        private const float C_DescDH = 14f;

        // Szachownicowa: [DescLeft=14 | Num=11 | Strip=4 | GRID | Strip=4 | GrpLbl=14]
        //                [TopGrpLbl=12 | Strip=4 | GRID | Strip=4 | Letters=10 | BotDesc=14]
        private const float S_Cell = 30f;
        private const float S_DescW = 14f;
        private const float S_NumW = 11f;
        private const float S_StripW = 4f;
        private const float S_GrpW = 14f;
        private const float S_TopH = 12f;
        private const float S_StripH = 4f;
        private const float S_LetH = 10f;
        private const float S_BotH = 14f;

        public BoardsDocument(List<Board> allBoards, int teamBoardId, int rivalBoardId)
        {
            _teamBoardId = teamBoardId;
            _rivalBoardId = rivalBoardId;
            _boardsToRender = new List<Board>();

            var rivalBoard = allBoards.FirstOrDefault(b => b.Boards_Id == rivalBoardId);
            if (rivalBoard != null) _boardsToRender.Add(rivalBoard);

            var teamBoard = allBoards.FirstOrDefault(b => b.Boards_Id == teamBoardId);
            if (teamBoard != null) _boardsToRender.Add(teamBoard);

            _boardsToRender.AddRange(
                allBoards.Where(b => b.Boards_Id != teamBoardId && b.Boards_Id != rivalBoardId));
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            var rivalBoardData = _boardsToRender.FirstOrDefault(b => b.Boards_Id == _rivalBoardId);
            var teamBoardData = _boardsToRender.FirstOrDefault(b => b.Boards_Id == _teamBoardId);

            // ── Oblicz OSOBNĄ skalę dla każdej planszy ─────────────────────────
            //
            // Slot = 1200×1200pt (połowa strony 2500pt z marginesem 50pt).
            // Każda plansza skalowana NIEZALEŻNIE tak żeby totalW=slot i totalH≤slot.
            // Pozycjonowanie: alignRight=false + alignBottom=true dla Q2
            //                 alignRight=true  + alignBottom=false dla Q3
            // → dolna krawędź Q2 i górna krawędź Q3 przy centrum strony.
            // Przerwa pionowa między planszami ≈ 28pt (akceptowalna).
            // Przerwa pozioma = 0pt (plansze wypełniają slot w poziomie).

            const float Slot = 1200f;

            // Skala kartezjańskiej (rivalBoard)
            float scaleC = 1f;
            if (rivalBoardData != null)
            {
                bool dd = !string.IsNullOrWhiteSpace(rivalBoardData.Description_Down);
                float rW = C_DescW + C_AxisW + C_NumW + rivalBoardData.Cols * C_Cell + C_NumW + C_AxisW;
                float rH = C_TopH + C_LetH + rivalBoardData.Rows * C_Cell + C_LetH + C_BotH + (dd ? C_DescDH : 0f);
                scaleC = MathF.Min(Slot / rW, Slot / rH);
            }

            // Skala szachownicowej (teamBoard)
            float scaleS = 1f;
            if (teamBoardData != null)
            {
                float rW = S_DescW + S_NumW + S_StripW + teamBoardData.Cols * S_Cell + S_StripW + S_GrpW;
                float rH = S_TopH + S_StripH + teamBoardData.Rows * S_Cell + S_StripH + S_LetH + S_BotH;
                scaleS = MathF.Min(Slot / rW, Slot / rH);
            }

            var templatesDir = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
            byte[]? logoBytes = TryReadBytes(Path.Combine(templatesDir, "Zasob_14x.png"));
            byte[]? logotypesBytes = TryReadBytes(Path.Combine(templatesDir, "Zasob_24x.png"));

            container.Page(page =>
            {
                page.Size(2500, 2500, Unit.Point);
                page.Margin(50);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn();
                        cols.RelativeColumn();
                    });

                    // Q1 (lewy-górny): logo – zachowanie proporcji
                    table.Cell().Height(1200).AlignLeft().AlignTop()
                        .Element(c =>
                        {
                            if (logoBytes != null)
                                c.Width(600).Height(600).Image(logoBytes).FitArea();
                        });

                    // Q2 (prawy-górny): rivalBoard (kartezjańska)
                    // alignRight=false: lewa krawędź planszy = lewa krawędź slotu
                    // alignBottom=true: dolna krawędź planszy = dół slotu = centrum strony
                    table.Cell().Height(1200)
                        .Element(c =>
                        {
                            if (rivalBoardData != null)
                                RenderCartesian(c, rivalBoardData, 1200f, 1200f,
                                    alignRight: false, alignBottom: true, S: scaleC);
                        });

                    // Q3 (lewy-dolny): teamBoard (szachownicowa)
                    // alignRight=true: prawa krawędź planszy = prawa krawędź slotu = centrum strony
                    // alignBottom=false: górna krawędź planszy = góra slotu
                    table.Cell().Height(1200)
                        .Element(c =>
                        {
                            if (teamBoardData != null)
                                RenderChess(c, teamBoardData, 1200f, 1200f,
                                    alignRight: true, alignBottom: false, S: scaleS);
                        });

                    // Q4 (prawy-dolny): logotypy – zachowanie proporcji, prawy-dolny róg
                    table.Cell().Height(1200).AlignRight().AlignBottom()
                        .Element(c =>
                        {
                            if (logotypesBytes != null)
                                c.Width(1000).Image(logotypesBytes).FitArea();
                        });
                });
            });

            // Pozostałe plansze parami
            var others = _boardsToRender
                .Where(b => b.Boards_Id != _teamBoardId && b.Boards_Id != _rivalBoardId)
                .ToList();

            for (int i = 0; i < others.Count; i += 2)
            {
                var first = others[i];
                var second = i + 1 < others.Count ? others[i + 1] : null;

                // Osobna skala dla każdej planszy na dodatkowych stronach
                float scaleF = 1f, scaleG = 1f;
                {
                    bool dd = !string.IsNullOrWhiteSpace(first.Description_Down);
                    float rW = C_DescW + C_AxisW + C_NumW + first.Cols * C_Cell + C_NumW + C_AxisW;
                    float rH = C_TopH + C_LetH + first.Rows * C_Cell + C_LetH + C_BotH + (dd ? C_DescDH : 0f);
                    scaleF = MathF.Min(Slot / rW, Slot / rH);
                }
                if (second != null)
                {
                    bool dd = !string.IsNullOrWhiteSpace(second.Description_Down);
                    float rW = C_DescW + C_AxisW + C_NumW + second.Cols * C_Cell + C_NumW + C_AxisW;
                    float rH = C_TopH + C_LetH + second.Rows * C_Cell + C_LetH + C_BotH + (dd ? C_DescDH : 0f);
                    scaleG = MathF.Min(Slot / rW, Slot / rH);
                }

                container.Page(page =>
                {
                    page.Size(2500, 2500, Unit.Point);
                    page.Margin(50);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                        });

                        table.Cell().Height(1200);
                        table.Cell().Height(1200)
                            .Element(c => RenderCartesian(c, first, 1200f, 1200f,
                                false, true, scaleF));

                        table.Cell().Height(1200)
                            .Element(c =>
                            {
                                if (second != null)
                                    RenderCartesian(c, second, 1200f, 1200f,
                                        true, false, scaleG);
                            });
                        table.Cell().Height(1200);
                    });
                });
            }
        }

        private static byte[]? TryReadBytes(string path)
        {
            try { return File.Exists(path) ? File.ReadAllBytes(path) : null; }
            catch { return null; }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // PLANSZA KARTEZJAŃSKA
        // ═══════════════════════════════════════════════════════════════════════
        private static void RenderCartesian(IContainer container, Board board,
            float cellW, float cellH, bool alignRight, bool alignBottom, float S)
        {
            int halfCols = board.Cols / 2;
            int halfRows = board.Rows / 2;
            bool hasDD = !string.IsNullOrWhiteSpace(board.Description_Down);

            var qColors = SplitSemi(board.Borders_Colors, 4)
                            .Select(s => ParseColor(s, SKColors.LightGray)).ToArray();
            var qNames = SplitSemi(board.Cells_Descriptions, 4);
            var lblUp = SplitSemi(board.Labels_Up, 4);
            var lblR = SplitSemi(board.Labels_Right, 4);
            var border = ParseColor(board.Border_Color, SKColors.DarkGray);
            var cell = ParseColor(board.Cell_Color, SKColors.White);

            float cs = C_Cell * S;
            float dw = C_DescW * S;
            float aw = C_AxisW * S;
            float nw = C_NumW * S;
            float lh = C_LetH * S;
            float th = C_TopH * S;
            float bh = C_BotH * S;
            float ddh = hasDD ? C_DescDH * S : 0f;

            float bW = board.Cols * cs;
            float bH = board.Rows * cs;
            float ox = dw + aw + nw;
            float oy = th + lh;

            float totalW = dw + aw + nw + bW + nw + aw;
            float totalH = th + lh + bH + lh + bh + (hasDD ? ddh : 0f);

            float tx = alignRight ? cellW - totalW : 0f;
            float ty = alignBottom ? cellH - totalH : 0f;

            container.Width(cellW).Height(cellH)
                .SkiaSharpSvgCanvas((canvas, _) =>
            {
                canvas.Translate(tx, ty);

                // 1. Tła ćwiartek
                var qR = new[]
                {
                    SKRect.Create(ox,            oy,            halfCols*cs, halfRows*cs),
                    SKRect.Create(ox+halfCols*cs, oy,            halfCols*cs, halfRows*cs),
                    SKRect.Create(ox+halfCols*cs, oy+halfRows*cs,halfCols*cs, halfRows*cs),
                    SKRect.Create(ox,            oy+halfRows*cs,halfCols*cs, halfRows*cs),
                };
                for (int q = 0; q < 4; q++)
                {
                    using var f = new SKPaint { Color = qColors[q], IsAntialias = false };
                    canvas.DrawRect(qR[q], f);
                }

                // 2. Siatka
                using var thin = new SKPaint { Color = new SKColor(border.Red, border.Green, border.Blue, 90), StrokeWidth = 0.4f * S, IsStroke = true };
                using var thick = new SKPaint { Color = new SKColor(border.Red, border.Green, border.Blue, 200), StrokeWidth = 1.5f * S, IsStroke = true };
                for (int c = 0; c <= board.Cols; c++)
                    canvas.DrawLine(ox + c * cs, oy, ox + c * cs, oy + bH, c == halfCols ? thick : thin);
                for (int r = 0; r <= board.Rows; r++)
                    canvas.DrawLine(ox, oy + r * cs, ox + bW, oy + r * cs, r == halfRows ? thick : thin);

                // 3. Nazwy ćwiartek
                var lc = new SKColor(cell.Red, cell.Green, cell.Blue, 200);
                for (int q = 0; q < 4; q++)
                    DrawWrapped(canvas, qNames[q], qR[q].MidX, qR[q].MidY, lc, 14f * S, qR[q].Width - 6f * S);

                // 4. Litery kolumn
                using var lf = new SKFont(SKTypeface.Default, 8f * S);
                using var lp = new SKPaint { Color = new SKColor(50, 50, 50), IsAntialias = true };
                for (int c = 0; c < board.Cols; c++)
                {
                    float cxl = ox + (c + 0.5f) * cs;
                    string letter = (c < halfCols
                        ? (char)('A' + halfCols - 1 - c)
                        : (char)('A' + c - halfCols)).ToString();
                    canvas.DrawText(letter, cxl, oy - 2f * S, SKTextAlign.Center, lf, lp);
                    canvas.DrawText(letter, cxl, oy + bH + lf.Size + 2f * S, SKTextAlign.Center, lf, lp);
                }

                // 5. Numery wierszy
                using var nf = new SKFont(SKTypeface.Default, 8f * S);
                using var np = new SKPaint { Color = new SKColor(50, 50, 50), IsAntialias = true };
                for (int r = 0; r < board.Rows; r++)
                {
                    float cyr = oy + (r + 0.5f) * cs + nf.Size * 0.35f;
                    string num = (r < halfRows ? halfRows - r : r - halfRows + 1).ToString();
                    canvas.DrawText(num, ox - 2f * S, cyr, SKTextAlign.Right, nf, np);
                    canvas.DrawText(num, ox + bW + 2f * S, cyr, SKTextAlign.Left, nf, np);
                }

                // 6. Etykiety labelsUp – tuż nad/pod literami
                float qHW = halfCols * cs * 0.5f;
                float[] upX = { ox + qHW, ox + halfCols * cs + qHW, ox + qHW, ox + halfCols * cs + qHW };
                float topLblY = oy - lh - 2f * S;
                float botLblY = oy + bH + lh + bh - 2f * S;
                float[] upY = { topLblY, topLblY, botLblY, botLblY };

                using var uf = new SKFont(SKTypeface.Default, 8f * S);
                using var up2 = new SKPaint { Color = new SKColor(50, 50, 50), IsAntialias = true };
                for (int i = 0; i < 4; i++)
                    canvas.DrawText(lblUp[i], upX[i], upY[i], SKTextAlign.Center, uf, up2);

                // 7. Etykiety labelsRight – obrócone –90°, środek paska aw
                float halfBH = halfRows * cs;
                float leftCx = dw + aw * 0.5f;
                float rightCx = ox + bW + nw + aw * 0.5f;
                float[] sX = { leftCx, leftCx, rightCx, rightCx };
                float[] sY =
                {
                    oy + halfBH*0.5f,
                    oy + halfBH + halfBH*0.5f,
                    oy + halfBH*0.5f,
                    oy + halfBH + halfBH*0.5f,
                };
                for (int i = 0; i < 4; i++)
                {
                    canvas.Save();
                    canvas.RotateDegrees(-90f, sX[i], sY[i]);
                    DrawWrapped(canvas, lblR[i], sX[i], sY[i],
                        new SKColor(50, 50, 50), 7f * S, halfBH - 4f * S);
                    canvas.Restore();
                }

                // 8. Description_Left
                if (!string.IsNullOrWhiteSpace(board.Description_Left))
                {
                    float midY = oy + bH / 2f;
                    using var dlf = new SKFont(SKTypeface.FromFamilyName(null, SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), 9f * S);
                    using var dlp = new SKPaint { Color = new SKColor(30, 30, 30), IsAntialias = true };
                    canvas.Save();
                    canvas.RotateDegrees(-90f, dw * 0.5f, midY);
                    canvas.DrawText(board.Description_Left, dw * 0.5f, midY, SKTextAlign.Center, dlf, dlp);
                    canvas.Restore();
                }

                // 9. Description_Down
                if (hasDD)
                {
                    float totalWLocal = dw + aw + nw + bW + nw + aw;
                    float dy = oy + bH + lh + bh + ddh - 2f * S;
                    using var ddf = new SKFont(SKTypeface.FromFamilyName(null, SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), 9f * S);
                    using var ddp = new SKPaint { Color = new SKColor(30, 30, 30), IsAntialias = true };
                    canvas.DrawText(board.Description_Down, totalWLocal / 2f, dy, SKTextAlign.Center, ddf, ddp);
                }

                // 10. Środkowe kółko
                using var cf = new SKPaint { Color = SKColors.White, IsAntialias = true };
                using var cst = new SKPaint { Color = border, StrokeWidth = 1.5f * S, IsStroke = true, IsAntialias = true };
                canvas.DrawCircle(ox + halfCols * cs, oy + halfRows * cs, 8f * S, cf);
                canvas.DrawCircle(ox + halfCols * cs, oy + halfRows * cs, 8f * S, cst);
            });
        }

        // ═══════════════════════════════════════════════════════════════════════
        // PLANSZA SZACHOWNICOWA
        // ═══════════════════════════════════════════════════════════════════════
        private static void RenderChess(IContainer container, Board board,
            float cellW, float cellH, bool alignRight, bool alignBottom, float S)
        {
            int cols = board.Cols;
            int rows = board.Rows;

            var seg = SplitSemi(board.Borders_Colors, 4)
                            .Select(s => ParseColor(s, SKColors.LightGray)).ToArray();
            var lblUp = SplitSemi(board.Labels_Up, seg.Length);
            var lblR = SplitSemi(board.Labels_Right, seg.Length);
            var cell = ParseColor(board.Cell_Color, new SKColor(255, 253, 230));
            var border = ParseColor(board.Border_Color, SKColors.DarkGray);

            int ng = seg.Length;
            int cpg = cols / ng;
            int rpg = rows / ng;
            bool hasDD = !string.IsNullOrWhiteSpace(board.Description_Down);

            float cs = S_Cell * S;
            float dw = S_DescW * S;
            float nw = S_NumW * S;
            float sw = S_StripW * S;
            float gw = S_GrpW * S;
            float tgh = S_TopH * S;
            float sh = S_StripH * S;
            float lh = S_LetH * S;
            float bdh = S_BotH * S;

            float bW = cols * cs;
            float bH = rows * cs;
            float ox = dw + nw + sw;
            float oy = tgh + sh;

            float totalW = dw + nw + sw + bW + sw + gw;
            float totalH = tgh + sh + bH + sh + lh + bdh;

            float tx = alignRight ? cellW - totalW : 0f;
            float ty = alignBottom ? cellH - totalH : 0f;

            container.Width(cellW).Height(cellH)
                .SkiaSharpSvgCanvas((canvas, _) =>
            {
                canvas.Translate(tx, ty);

                // 1. Tło komórek
                using var cf = new SKPaint { Color = cell, IsAntialias = false };
                canvas.DrawRect(SKRect.Create(ox, oy, bW, bH), cf);

                // 2. Siatka
                using var gp = new SKPaint { Color = new SKColor(border.Red, border.Green, border.Blue, 80), StrokeWidth = 0.3f * S, IsStroke = true };
                for (int c = 0; c <= cols; c++) canvas.DrawLine(ox + c * cs, oy, ox + c * cs, oy + bH, gp);
                for (int r = 0; r <= rows; r++) canvas.DrawLine(ox, oy + r * cs, ox + bW, oy + r * cs, gp);

                // 3. Paski kolumn (góra/dół)
                for (int g = 0; g < ng; g++)
                {
                    using var sf = new SKPaint { Color = seg[g], IsAntialias = false };
                    float gx = ox + g * cpg * cs;
                    float gww = cpg * cs;
                    canvas.DrawRect(SKRect.Create(gx, oy - sh, gww, sh), sf);
                    canvas.DrawRect(SKRect.Create(gx, oy + bH, gww, sh), sf);
                }

                // 4. Paski wierszy (lewo/prawo)
                for (int g = 0; g < ng; g++)
                {
                    using var sf = new SKPaint { Color = seg[g], IsAntialias = false };
                    float gy = oy + (rows - (g + 1) * rpg) * cs;
                    float ght = rpg * cs;
                    canvas.DrawRect(SKRect.Create(ox - sw, gy, sw, ght), sf);
                    canvas.DrawRect(SKRect.Create(ox + bW, gy, sw, ght), sf);
                }

                // 5. Etykiety grup kolumn
                using var tgf = new SKFont(SKTypeface.Default, 8f * S);
                using var tgp = new SKPaint { Color = new SKColor(40, 40, 40), IsAntialias = true };
                for (int g = 0; g < ng; g++)
                {
                    float gx = ox + g * cpg * cs;
                    float gww = cpg * cs;
                    float lblY = tgh * 0.5f + tgf.Size * 0.35f;
                    canvas.DrawText(lblUp[g], gx + gww * 0.5f, lblY, SKTextAlign.Center, tgf, tgp);
                }

                // 6. Etykiety grup wierszy – obrócone –90°
                using var rgf = new SKFont(SKTypeface.Default, 8f * S);
                using var rgp = new SKPaint { Color = SKColors.Black, IsAntialias = true };
                float rCx = ox + bW + sw + gw * 0.5f;
                for (int g = 0; g < ng; g++)
                {
                    float gy = oy + (rows - (g + 1) * rpg) * cs;
                    float ght = rpg * cs;
                    float midY = gy + ght * 0.5f;
                    canvas.Save();
                    canvas.RotateDegrees(-90f, rCx, midY);
                    canvas.DrawText(lblR[g], rCx, midY + rgf.Size * 0.35f, SKTextAlign.Center, rgf, rgp);
                    canvas.Restore();
                }

                // 7. Litery kolumn
                using var clf = new SKFont(SKTypeface.Default, 8f * S);
                using var clp = new SKPaint { Color = new SKColor(60, 60, 60), IsAntialias = true };
                for (int c = 0; c < cols; c++)
                {
                    float lx = ox + (c + 0.5f) * cs;
                    float ly = oy + bH + sh + clf.Size + 2f * S;
                    canvas.DrawText(((char)('A' + c)).ToString(), lx, ly, SKTextAlign.Center, clf, clp);
                }

                // 8. Numery wierszy
                using var rnf = new SKFont(SKTypeface.Default, 8f * S);
                using var rnp = new SKPaint { Color = new SKColor(60, 60, 60), IsAntialias = true };
                for (int r = 0; r < rows; r++)
                {
                    float ry = oy + (r + 0.5f) * cs + rnf.Size * 0.35f;
                    canvas.DrawText((rows - r).ToString(), ox - sw - 2f * S, ry, SKTextAlign.Right, rnf, rnp);
                }

                // 9. Description_Left
                if (!string.IsNullOrWhiteSpace(board.Description_Left))
                {
                    float midY = oy + bH / 2f;
                    using var dlf = new SKFont(SKTypeface.FromFamilyName(null, SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), 9f * S);
                    using var dlp = new SKPaint { Color = new SKColor(40, 40, 40), IsAntialias = true };
                    canvas.Save();
                    canvas.RotateDegrees(-90f, dw * 0.5f, midY);
                    canvas.DrawText(board.Description_Left, dw * 0.5f, midY, SKTextAlign.Center, dlf, dlp);
                    canvas.Restore();
                }

                // 10. Description_Down
                if (hasDD)
                {
                    float totalWLocal = dw + nw + sw + bW + sw + gw;
                    float dy = oy + bH + sh + lh + bdh - 3f * S;
                    using var ddf = new SKFont(SKTypeface.FromFamilyName(null, SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), 9f * S);
                    using var ddp = new SKPaint { Color = new SKColor(40, 40, 40), IsAntialias = true };
                    canvas.DrawText(board.Description_Down, totalWLocal / 2f, dy, SKTextAlign.Center, ddf, ddp);
                }
            });
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static string[] SplitSemi(string? raw, int count)
        {
            var arr = (raw ?? "").Split(';', StringSplitOptions.None)
                                 .Select(s => s.Trim()).ToArray();
            return arr.Length >= count
                ? arr.Take(count).ToArray()
                : arr.Concat(Enumerable.Repeat(string.Empty, count - arr.Length)).ToArray();
        }

        private static SKColor ParseColor(string? hex, SKColor fallback)
        {
            if (string.IsNullOrWhiteSpace(hex)) return fallback;
            try { return SKColor.Parse(hex.Trim()); }
            catch { return fallback; }
        }

        private static void DrawWrapped(SKCanvas canvas, string text,
            float cx, float cy, SKColor color, float size, float maxWidth)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            using var font = new SKFont(SKTypeface.Default, size);
            using var paint = new SKPaint { Color = color, IsAntialias = true };

            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var lines = new List<string>();
            var cur = "";
            foreach (var w in words)
            {
                var c = cur.Length == 0 ? w : cur + " " + w;
                if (font.MeasureText(c) <= maxWidth) cur = c;
                else { if (cur.Length > 0) lines.Add(cur); cur = w; }
            }
            if (cur.Length > 0) lines.Add(cur);

            float lineH = size * 1.3f;
            float startY = cy - (lines.Count - 1) * lineH / 2f + size * 0.35f;
            for (int i = 0; i < lines.Count; i++)
                canvas.DrawText(lines[i], cx, startY + i * lineH, SKTextAlign.Center, font, paint);
        }
    }
}