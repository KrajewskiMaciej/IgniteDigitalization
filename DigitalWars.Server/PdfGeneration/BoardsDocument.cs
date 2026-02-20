using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using backend.Data;

namespace backend.PdfGeneration
{
    public class BoardDrawingConfig
    {
        public float CellSize { get; } = 30;
        public float RowLabelWidth { get; } = 30;
        public float ColumnLabelHeight { get; } = 20;
        public float TopLabelHeight { get; } = 20;
        public float RightLabelWidth { get; } = 30;
        public float BorderWidth { get; } = 4;
        public int Rows { get; }
        public int Cols { get; }
        public string BorderColors { get; } = string.Empty;
        public string BorderColor { get; }
        public string CellColor { get; }

        public BoardDrawingConfig(Board board)
        {
            Rows = board.Rows;
            Cols = board.Cols;
            BorderColor = board.Border_Color;
            CellColor = board.Cell_Color;
            BorderColors = board.Borders_Colors;
        }
    }

    public class BoardsDocument : IDocument
    {
        private readonly List<Board> _boardsToRender;

        public BoardsDocument(List<Board> allBoards, int teamBoardId, int rivalBoardId)
        {
            _boardsToRender = new List<Board>();

            var rivalBoard = allBoards.FirstOrDefault(b => b.Boards_Id == rivalBoardId);
            if (rivalBoard != null) _boardsToRender.Add(rivalBoard);

            var teamBoard = allBoards.FirstOrDefault(b => b.Boards_Id == teamBoardId);
            if (teamBoard != null) _boardsToRender.Add(teamBoard);

            var otherBoards = allBoards.Where(b => b.Boards_Id != teamBoardId && b.Boards_Id != rivalBoardId);
            _boardsToRender.AddRange(otherBoards);
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            for (int i = 0; i < _boardsToRender.Count; i += 2)
            {
                var firstBoardInPair = _boardsToRender[i];
                var secondBoardInPair = (i + 1 < _boardsToRender.Count) ? _boardsToRender[i + 1] : null;

                container.Page(page =>
                {
                    page.Size(2000, 2000, Unit.Point);
                    page.Margin(50);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Cell();

                        table.Cell()
                            .AlignCenter()
                            .AlignMiddle()
                            .PaddingLeft(-150)
                            .PaddingBottom(-90)
                            .Element(c => ComposeBoard(c, firstBoardInPair));

                        table.Cell()
                            .AlignCenter()
                            .AlignMiddle()
                            .PaddingRight(-150)
                            .PaddingTop(-90)
                            .Element(c =>
                            {
                                if (secondBoardInPair != null)
                                {
                                    ComposeBoard(c, secondBoardInPair);
                                }
                            });

                        table.Cell();
                    });
                });
            }
        }

        private void ComposeBoard(IContainer container, Board boardData)
        {
            var config = new BoardDrawingConfig(boardData);

            container.Scale(3.2f).Column(mainColumn =>
            {
                mainColumn.Spacing(5);

                mainColumn.Item()
                .AlignCenter()
                .Row(boardRow =>
                {
                    boardRow.Spacing(5);

                    if (!string.IsNullOrEmpty(boardData.Description_Left))
                    {
                        boardRow.AutoItem()
                            .RotateLeft()
                            .AlignCenter()
                            .Text(boardData.Description_Left)
                            .FontSize(14).Bold();
                    }

                    boardRow.AutoItem().Column(boardColumn =>
                    {
                        if (!string.IsNullOrEmpty(boardData.Labels_Up))
                        {
                            boardColumn.Item().Element(c => DrawTopLabels(c, config, boardData.Labels_Up));
                        }

                        boardColumn.Item().AlignCenter().Element(c => DrawHorizontalBorder(c, config));

                        boardColumn.Item().Row(coreRow =>
                        {
                            coreRow.ConstantItem(config.RowLabelWidth).Column(numbers =>
                            {
                                for (int r = 0; r < config.Rows; r++)
                                {
                                    numbers.Item().Height(config.CellSize).AlignCenter().AlignMiddle()
                                           .Text((config.Rows - r).ToString());
                                }
                            });

                            coreRow.AutoItem().Element(c => DrawVerticalBorder(c, config));

                            coreRow.ConstantItem(config.Cols * config.CellSize).Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    for (int i = 0; i < config.Cols; i++)
                                        cols.ConstantColumn(config.CellSize);
                                });
                                for (int r = 0; r < config.Rows; r++)
                                    for (int c = 0; c < config.Cols; c++)
                                        table.Cell().Row((uint)r + 1).Column((uint)c + 1)
                                             .Border(0.5f).BorderColor(config.BorderColor)
                                             .Background(config.CellColor).Height(config.CellSize);
                            });

                            coreRow.AutoItem().Element(c => DrawVerticalBorder(c, config));

                            if (!string.IsNullOrEmpty(boardData.Labels_Right))
                            {
                                coreRow.ConstantItem(config.RightLabelWidth).Column(rightLabels =>
                                {
                                    var labels = boardData.Labels_Right.Split(';');
                                    for (int i = 0; i < config.Rows; i += 2)
                                    {
                                        var labelIndex = i / 2;
                                        var item = rightLabels.Item().Height(config.CellSize * 2);
                                        if (labelIndex < labels.Length)
                                        {
                                            item.AlignCenter().AlignMiddle().RotateLeft()
                                                .Text(labels[labelIndex].Trim()).FontSize(8);
                                        }
                                    }
                                });
                            }
                        });

                        boardColumn.Item().AlignCenter().Element(c => DrawHorizontalBorder(c, config));
                        boardColumn.Item().Element(c => DrawBottomLabels(c, config));
                    });
                });

                if (!string.IsNullOrEmpty(boardData.Description_Down))
                {
                    mainColumn.Item().AlignCenter().Text(boardData.Description_Down).FontSize(14).Bold();
                }
            });
        }

        private void DrawTopLabels(IContainer container, BoardDrawingConfig config, string labelsUp)
        {
            container.Row(row =>
            {
                row.ConstantItem(config.BorderWidth + config.RowLabelWidth);

                var labels = labelsUp.Split(';');
                for (int i = 0; i < config.Cols; i += 2)
                {
                    var labelIndex = i / 2;
                    var item = row.ConstantItem(config.CellSize * 2).Height(config.TopLabelHeight);
                    if (labelIndex < labels.Length)
                    {
                        item.AlignCenter().Text(labels[labelIndex].Trim()).FontSize(8);
                    }
                }
            });
        }

        private void DrawBottomLabels(IContainer container, BoardDrawingConfig config)
        {
            container.Row(row =>
            {
                row.ConstantItem(config.BorderWidth + config.RowLabelWidth);

                for (int i = 0; i < config.Cols; i++)
                {
                    row.ConstantItem(config.CellSize).Height(config.ColumnLabelHeight)
                       .AlignCenter().Text(((char)('A' + i)).ToString());
                }
            });
        }

        private void DrawHorizontalBorder(IContainer container, BoardDrawingConfig config)
        {
            if (string.IsNullOrEmpty(config.BorderColors)) return;
            var colors = config.BorderColors.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (colors.Length == 0) return;

            container.Height(config.BorderWidth).Row(row =>
            {
                row.ConstantItem(config.RowLabelWidth);
                for (int i = 0; i < config.Cols; i += 2)
                {
                    var colorIndex = (i / 2) % colors.Length;
                    row.ConstantItem(config.CellSize * 2).Background(colors[colorIndex]);
                }
                row.ConstantItem(config.RightLabelWidth);
            });
        }

        private void DrawVerticalBorder(IContainer container, BoardDrawingConfig config)
        {
            if (string.IsNullOrEmpty(config.BorderColors)) return;
            var colors = config.BorderColors.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (colors.Length == 0) return;

            container.Width(config.BorderWidth).Column(column =>
            {
                for (int i = config.Rows - 2; i >= 0; i -= 2)
                {
                    var colorIndex = (i / 2) % colors.Length;
                    column.Item().Height(config.CellSize * 2).Background(colors[colorIndex]);
                }
            });
        }
    }
}