using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using PdfPigDocument = UglyToad.PdfPig.PdfDocument;

namespace Lbe.Documents;

/// <summary>Exports a tabular report to DOCX, XLSX, or PDF and extracts text from an existing PDF.</summary>
public static class DocumentExporter
{
    /// <summary>Creates a Word document with a heading and data table.</summary>
    public static void WriteDocx(TabularReport report, Stream destination)
    {
        ValidateOutput(report, destination);
        using var document = WordprocessingDocument.Create(destination, WordprocessingDocumentType.Document, true);
        var main = document.AddMainDocumentPart();
        var body = new Body();
        main.Document = new Document(body);

        body.AppendChild(new Paragraph(
            new ParagraphProperties(new ParagraphStyleId { Val = "Title" }),
            new Run(new Text(report.Title))));

        var table = new Table();
        table.AppendChild(new TableProperties(
            new TableBorders(
                new TopBorder { Val = BorderValues.Single, Size = 4 },
                new BottomBorder { Val = BorderValues.Single, Size = 4 },
                new LeftBorder { Val = BorderValues.Single, Size = 4 },
                new RightBorder { Val = BorderValues.Single, Size = 4 },
                new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
                new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 })));

        var heading = new TableRow();
        foreach (var column in report.Columns)
            heading.AppendChild(new TableCell(new Paragraph(new Run(new RunProperties(new Bold()), new Text(column)))));
        table.AppendChild(heading);

        foreach (var values in report.Rows)
        {
            var row = new TableRow();
            foreach (var value in values)
                row.AppendChild(new TableCell(new Paragraph(new Run(new Text(value)))));
            table.AppendChild(row);
        }

        body.AppendChild(table);
        main.Document.Save();
    }

    /// <summary>Creates an Excel workbook. Cell values are stored as text.</summary>
    public static void WriteXlsx(TabularReport report, Stream destination)
    {
        ValidateOutput(report, destination);
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Reporte");
        sheet.Cell(1, 1).Value = report.Title;
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 16;

        for (var column = 0; column < report.Columns.Count; column++)
            sheet.Cell(3, column + 1).Value = report.Columns[column];

        for (var row = 0; row < report.Rows.Count; row++)
            for (var column = 0; column < report.Columns.Count; column++)
                sheet.Cell(row + 4, column + 1).Value = report.Rows[row][column];

        var lastRow = Math.Max(3, report.Rows.Count + 3);
        var table = sheet.Range(3, 1, lastRow, report.Columns.Count).CreateTable();
        table.Theme = XLTableTheme.TableStyleMedium2;
        sheet.Columns(1, report.Columns.Count).AdjustToContents();
        workbook.SaveAs(destination);
    }

    /// <summary>Writes a PDF using the supplied font. Configure PDFsharp's font resolver before constructing the font on Linux.</summary>
    public static void WritePdf(TabularReport report, Stream destination, XFont font)
    {
        ValidateOutput(report, destination);
        ArgumentNullException.ThrowIfNull(font);

        const double margin = 40;
        const double lineHeight = 19;
        using var document = new PdfDocument();
        PdfPage? page = null;
        XGraphics? graphics = null;
        var y = 0.0;
        var columnWidth = 0.0;

        void NewPage()
        {
            graphics?.Dispose();
            page = document.AddPage();
            graphics = XGraphics.FromPdfPage(page);
            y = margin;
            columnWidth = (page.Width.Point - margin * 2) / report.Columns.Count;
            graphics.DrawString(report.Title, font, XBrushes.Black, margin, y);
            y += lineHeight * 2;
            DrawRow(report.Columns, true);
        }

        void DrawRow(IReadOnlyList<string> values, bool header)
        {
            if (graphics is null || page is null)
                throw new InvalidOperationException("The PDF page has not been initialized.");
            if (!header && y + lineHeight > page.Height.Point - margin)
            {
                NewPage();
                return;
            }

            for (var column = 0; column < values.Count; column++)
            {
                var x = margin + column * columnWidth;
                var cell = new XRect(x + 4, y, columnWidth - 8, lineHeight);
                graphics.DrawString(FitText(graphics, values[column], font, cell.Width), font,
                    header ? XBrushes.DarkBlue : XBrushes.Black, cell, XStringFormats.CenterLeft);
            }
            graphics.DrawLine(XPens.LightGray, margin, y + lineHeight, page.Width.Point - margin, y + lineHeight);
            y += lineHeight;
        }

        NewPage();
        foreach (var row in report.Rows)
        {
            if (page is not null && y + lineHeight > page.Height.Point - margin)
                NewPage();
            DrawRow(row, false);
        }
        graphics?.Dispose();
        document.Save(destination, false);
    }

    /// <summary>Reads the text layer of a PDF. Image-only scans require OCR.</summary>
    public static string ExtractPdfText(Stream source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!source.CanRead || !source.CanSeek)
            throw new ArgumentException("The PDF stream must be readable and seekable.", nameof(source));

        using var document = PdfPigDocument.Open(source);
        return string.Join(Environment.NewLine + Environment.NewLine,
            document.GetPages().Select(page => ContentOrderTextExtractor.GetText(page)));
    }

    private static void ValidateOutput(TabularReport report, Stream destination)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(destination);
        if (!destination.CanWrite)
            throw new ArgumentException("The destination stream must be writable.", nameof(destination));
    }

    private static string FitText(XGraphics graphics, string text, XFont font, double width)
    {
        if (graphics.MeasureString(text, font).Width <= width)
            return text;
        const string ellipsis = "…";
        var length = text.Length;
        while (length > 0 && graphics.MeasureString(text[..length] + ellipsis, font).Width > width)
            length--;
        return length == 0 ? ellipsis : text[..length] + ellipsis;
    }
}
