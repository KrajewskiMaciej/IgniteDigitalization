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
        // Kartezjańska: [DescLeft | AxisLbl | Num | GRID | Num | AxisLbl]
        //               [TopLbl | Letters  | GRID | Letters | BotLbl | DescDown]
        private const float C_Cell = 30f;
        private const float C_DescW = 14f;   // Description_Left
        private const float C_AxisW = 12f;   // labelsRight boczne (obrócone)
        private const float C_NumW = 11f;   // numery wierszy
        private const float C_LetH = 10f;   // litery kolumn
        private const float C_TopH = 11f;   // labelsUp nad siatką
        private const float C_BotH = 11f;   // labelsUp pod siatką
        private const float C_DescDH = 14f;   // Description_Down

        // Szachownicowa: [DescLeft | Num | Strip | GRID | Strip | GrpLbl]
        //                [TopGrpLbl | Strip | GRID | Strip | Letters | BotDesc]
        private const float S_Cell = 30f;
        private const float S_DescW = 14f;
        private const float S_NumW = 11f;
        private const float S_StripW = 4f;
        private const float S_GrpW = 14f;   // etykiety grup wierszy
        private const float S_TopH = 12f;   // etykiety grup kolumn
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

            // ── Oblicz skalę na podstawie maksymalnego możliwego rozmiaru siatki ──
            //
            // Strona: 2500×2500pt, margines 50pt → canvas 2400×2400pt
            // Centrum canvas: (1200, 1200)
            //
            // Q2 (kartezjańska, prawy-górny od centrum):
            //   Siatka od centrum w prawo i w górę.
            //   Lewy brzeg siatki = centrum_x (x=1200), dolny brzeg siatki = centrum_y (y=1200)
            //   Marginesy lewe  (ox_C) wychodzą na lewo od centrum → mogą sięgać do x=0
            //   Marginesy górne (oy_C) wychodzą ponad siatkę, siatka sięga ponad centrum
            //
            // Q3 (szachownicowa, lewy-dolny od centrum):
            //   Siatka od centrum w lewo i w dół.
            //   Prawy brzeg siatki = centrum_x (x=1200), górny brzeg siatki = centrum_y (y=1200)
            //
            // Ograniczenia (gridSize = cols * C_Cell * S, zakładamy cols=rows=8):
            //   Q2: ox_C*S ≤ 1200          → S ≤ 1200/ox_C
            //   Q2: (oy_C + gridC)*S ≤ 1200 → (oy_C + cols*C_Cell)*S ≤ 1200
            //   Q2: (gridC + margR_C)*S ≤ 1200 (prawa strona planszy)
            //   Q3: (gridS + margR_S)*S ≤ 1200
            //   Q3: (gridS + margB_S)*S ≤ 1200
            //
            // Wyznaczamy S jako minimum wszystkich ograniczeń.

            float S = ComputeScale(rivalBoardData, teamBoardData);

            var templatesDir = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
            byte[]? logoBytes = TryReadBytes(Path.Combine(templatesDir, "Zasob_14x.png"));
            byte[]? logotypesBytes = TryReadBytes(Path.Combine(templatesDir, "Zasob_24x.png"));

            container.Page(page =>
            {
                // Strona 2500×2500, margines 50 → obszar roboczy 2400×2400
                // Centrum obszaru roboczego = (1200, 1200) w układzie SkiaSharp
                page.Size(2500, 2500, Unit.Point);
                page.Margin(50);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                page.Content()
                    .Width(2400).Height(2400)
                    .SkiaSharpSvgCanvas((canvas, size) =>
                {
                    float cx = size.Width / 2f;   // centrum X = 1200
                    float cy = size.Height / 2f;   // centrum Y = 1200

                    // Logo w lewym-górnym rogu
                    if (logoBytes != null)
                        DrawImage(canvas, logoBytes, 0, 0, 600, 600);

                    // Logotypy w prawym-dolnym rogu
                    if (logotypesBytes != null)
                        DrawImage(canvas, logotypesBytes, size.Width - 1000, size.Height - 120, 1000, 120);

                    // Q2 (prawy-górny): rivalBoard
                    // Siatka: lewy brzeg = cx, dolny brzeg = cy
                    if (rivalBoardData != null)
                        DrawCartesian(canvas, rivalBoardData, S, cx, cy, quadrant: 2);

                    // Q3 (lewy-dolny): teamBoard
                    // Siatka: prawy brzeg = cx, górny brzeg = cy
                    if (teamBoardData != null)
                        DrawChess(canvas, teamBoardData, S, cx, cy, quadrant: 3);
                });
            });

            // Pozostałe plansze parami na kolejnych stronach
            var others = _boardsToRender
                .Where(b => b.Boards_Id != _teamBoardId && b.Boards_Id != _rivalBoardId)
                .ToList();

            for (int i = 0; i < others.Count; i += 2)
            {
                var first = others[i];
                var second = i + 1 < others.Count ? others[i + 1] : null;

                container.Page(page =>
                {
                    page.Size(2500, 2500, Unit.Point);
                    page.Margin(50);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    page.Content()
                        .Width(2400).Height(2400)
                        .SkiaSharpSvgCanvas((canvas, size) =>
                    {
                        float cx = size.Width / 2f;
                        float cy = size.Height / 2f;
                        // Q2 (prawy-górny) i Q3 (lewy-dolny) – obie kartezjańskie
                        DrawCartesian(canvas, first, S, cx, cy, quadrant: 2);
                        if (second != null)
                            DrawCartesian(canvas, second, S, cx, cy, quadrant: 3);
                    });
                });
            }
        }

        // ── Oblicz maksymalną skalę ─────────────────────────────────────────
        private static float ComputeScale(Board? rivalBoard, Board? teamBoard)
        {
            float S = 999f;

            if (rivalBoard != null)
            {
                bool dd = !string.IsNullOrWhiteSpace(rivalBoard.Description_Down);
                int cols = rivalBoard.Cols;
                int rows = rivalBoard.Rows;

                // nieskalowane marginesy kartezjańskiej
                float ox = C_DescW + C_AxisW + C_NumW;   // margines lewy od siatki
                float oy = C_TopH + C_LetH;             // margines górny od siatki
                float mrC = C_NumW + C_AxisW;            // margines prawy od siatki
                float mbC = C_LetH + C_BotH + (dd ? C_DescDH : 0f); // margines dolny

                // Ograniczenia (centrum = 1200):
                // lewy margines mieści się od centrum do lewej: ox*S ≤ 1200
                S = MathF.Min(S, 1200f / ox);
                // siatka + górny margines mieszczą się od centrum do góry: (oy + rows*C_Cell)*S ≤ 1200
                S = MathF.Min(S, 1200f / (oy + rows * C_Cell));
                // siatka + prawy margines mieszczą się od centrum do prawej: (cols*C_Cell + mrC)*S ≤ 1200
                S = MathF.Min(S, 1200f / (cols * C_Cell + mrC));
                // dolny margines mieści się od centrum do dołu: mbC*S ≤ 1200
                S = MathF.Min(S, 1200f / MathF.Max(mbC, 1f));
            }

            if (teamBoard != null)
            {
                int cols = teamBoard.Cols;
                int rows = teamBoard.Rows;
                bool dd = !string.IsNullOrWhiteSpace(teamBoard.Description_Down);

                float ox = S_DescW + S_NumW + S_StripW;  // margines lewy
                float oy = S_TopH + S_StripH;           // margines górny
                float mrS = S_StripW + S_GrpW;            // margines prawy
                float mbS = S_StripH + S_LetH + (dd ? S_BotH : S_BotH); // margines dolny

                // Prawy margines + siatka od centrum do lewej: (cols*S_Cell + mrS)*S ≤ 1200
                S = MathF.Min(S, 1200f / (cols * S_Cell + mrS));
                // Siatka + dolny margines od centrum do dołu: (rows*S_Cell + mbS)*S ≤ 1200
                S = MathF.Min(S, 1200f / (rows * S_Cell + mbS));
                // Górny margines od centrum do góry: oy*S ≤ 1200
                S = MathF.Min(S, 1200f / MathF.Max(oy, 1f));
                // Lewy margines od centrum do lewej (razem z siatką po lewej): (cols*S_Cell + ox)*S ≤ 1200
                S = MathF.Min(S, 1200f / (cols * S_Cell + ox));
            }

            return S;
        }

        // ── Helper do rysowania obrazu przez SkiaSharp ──────────────────────
        private static void DrawImage(SKCanvas canvas, byte[] data, float x, float y, float w, float h)
        {
            using var bmp = SKBitmap.Decode(data);
            if (bmp == null) return;
            canvas.DrawBitmap(bmp, SKRect.Create(x, y, w, h));
        }

        private static byte[]? TryReadBytes(string path)
        {
            try { return File.Exists(path) ? File.ReadAllBytes(path) : null; }
            catch { return null; }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // PLANSZA KARTEZJAŃSKA
        // quadrant=2: lewy-dolny róg siatki = (cx, cy)  → siatka na prawo i w górę
        // quadrant=3: prawy-górny róg siatki = (cx, cy) → siatka na lewo i w dół
        // ═══════════════════════════════════════════════════════════════════════
        private static void DrawCartesian(SKCanvas canvas, Board board, float S,
            float cx, float cy, int quadrant)
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

            // Pozycja siatki w globalnym canvas:
            float gridX, gridY;
            if (quadrant == 2)
            {
                // Lewy-dolny róg siatki przy centrum
                gridX = cx;
                gridY = cy - bH;
            }
            else // quadrant == 3
            {
                // Prawy-górny róg siatki przy centrum
                gridX = cx - bW;
                gridY = cy;
            }

            // ox/oy = offsety siatki od lewego-górnego rogu planszy
            float ox = dw + aw + nw;
            float oy = th + lh;

            // Absolutna pozycja lewego-górnego rogu planszy
            float px = gridX - ox;
            float py = gridY - oy;

            canvas.Save();
            canvas.Translate(px, py);

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

            // 6. Etykiety labelsUp
            float qHW = halfCols * cs * 0.5f;
            float[] upX = { ox + qHW, ox + halfCols * cs + qHW, ox + qHW, ox + halfCols * cs + qHW };
            float topLblY = oy - lh - 2f * S;
            float botLblY = oy + bH + lh + bh - 2f * S;
            float[] upY = { topLblY, topLblY, botLblY, botLblY };

            using var uf = new SKFont(SKTypeface.Default, 8f * S);
            using var up2 = new SKPaint { Color = new SKColor(50, 50, 50), IsAntialias = true };
            for (int i = 0; i < 4; i++)
                canvas.DrawText(lblUp[i], upX[i], upY[i], SKTextAlign.Center, uf, up2);

            // 7. Etykiety labelsRight – obrócone –90°
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
                float totalW = dw + aw + nw + bW + nw + aw;
                float dy = oy + bH + lh + bh + ddh - 2f * S;
                using var ddf = new SKFont(SKTypeface.FromFamilyName(null, SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), 9f * S);
                using var ddp = new SKPaint { Color = new SKColor(30, 30, 30), IsAntialias = true };
                canvas.DrawText(board.Description_Down, totalW / 2f, dy, SKTextAlign.Center, ddf, ddp);
            }

            // 10. Środkowe kółko
            using var cf = new SKPaint { Color = SKColors.White, IsAntialias = true };
            using var cst = new SKPaint { Color = border, StrokeWidth = 1.5f * S, IsStroke = true, IsAntialias = true };
            canvas.DrawCircle(ox + halfCols * cs, oy + halfRows * cs, 8f * S, cf);
            canvas.DrawCircle(ox + halfCols * cs, oy + halfRows * cs, 8f * S, cst);

            canvas.Restore();
        }

        // ═══════════════════════════════════════════════════════════════════════
        // PLANSZA SZACHOWNICOWA
        // quadrant=3: prawy-górny róg siatki = (cx, cy) → siatka na lewo i w dół
        // ═══════════════════════════════════════════════════════════════════════
        private static void DrawChess(SKCanvas canvas, Board board, float S,
            float cx, float cy, int quadrant)
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

            float ox = dw + nw + sw;  // lewy offset siatki w planszy
            float oy = tgh + sh;      // górny offset siatki w planszy

            // Pozycja siatki w globalnym canvas
            float gridX = cx - bW;   // prawy brzeg siatki = cx
            float gridY = cy;        // górny brzeg siatki = cy

            float px = gridX - ox;   // lewy-górny róg planszy
            float py = gridY - oy;

            canvas.Save();
            canvas.Translate(px, py);

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
                float totalW = dw + nw + sw + bW + sw + gw;
                float dy = oy + bH + sh + lh + bdh - 3f * S;
                using var ddf = new SKFont(SKTypeface.FromFamilyName(null, SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), 9f * S);
                using var ddp = new SKPaint { Color = new SKColor(40, 40, 40), IsAntialias = true };
                canvas.DrawText(board.Description_Down, totalW / 2f, dy, SKTextAlign.Center, ddf, ddp);
            }

            canvas.Restore();
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