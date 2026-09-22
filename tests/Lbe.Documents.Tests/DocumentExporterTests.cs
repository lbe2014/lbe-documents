using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using Lbe.Documents;
using Xunit;

namespace Lbe.Documents.Tests;

public sealed class DocumentExporterTests
{
    private static TabularReport Sample => new("Pedidos", ["Id", "Cliente"], [["A-001", "Ana"], ["A-002", "Luis"]]);

    [Fact]
    public void Docx_contains_title_and_rows()
    {
        using var output = new MemoryStream();
        DocumentExporter.WriteDocx(Sample, output);
        output.Position = 0;
        using var document = WordprocessingDocument.Open(output, false);
        var body = document.MainDocumentPart?.Document?.Body
            ?? throw new InvalidDataException("The generated DOCX has no body.");
        var text = body.InnerText;
        Assert.Contains("Pedidos", text);
        Assert.Contains("A-002", text);
    }

    [Fact]
    public void Xlsx_contains_title_and_rows()
    {
        using var output = new MemoryStream();
        DocumentExporter.WriteXlsx(Sample, output);
        output.Position = 0;
        using var workbook = new XLWorkbook(output);
        var sheet = workbook.Worksheet("Reporte");
        Assert.Equal("Pedidos", sheet.Cell(1, 1).GetString());
        Assert.Equal("Cliente", sheet.Cell(3, 2).GetString());
        Assert.Equal("Luis", sheet.Cell(5, 2).GetString());
    }

    [Fact]
    public void Rejects_mismatched_rows()
    {
        Assert.Throws<ArgumentException>(() => new TabularReport("Pedidos", ["Id", "Cliente"], [["A-001"]]));
    }

    [Fact]
    public void Xlsx_does_not_turn_user_text_into_a_formula()
    {
        var report = new TabularReport("Datos", ["Valor"], [["=1+1"]]);
        using var output = new MemoryStream();
        DocumentExporter.WriteXlsx(report, output);
        output.Position = 0;
        using var workbook = new XLWorkbook(output);
        var cell = workbook.Worksheet("Reporte").Cell(4, 1);
        Assert.Equal("=1+1", cell.GetString());
        Assert.False(cell.HasFormula);
    }
}
