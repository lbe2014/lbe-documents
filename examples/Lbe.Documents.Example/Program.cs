using Lbe.Documents;
using PdfSharp.Drawing;
using PdfSharp.Fonts;

if (args.Length != 2 || !File.Exists(args[1]))
{
    Console.Error.WriteLine("Uso: dotnet run --project examples/Lbe.Documents.Example -- <salida> <fuente-ttf>");
    return 1;
}

var outputDirectory = Path.GetFullPath(args[0]);
Directory.CreateDirectory(outputDirectory);

var report = new TabularReport("Pedidos de ejemplo",
    ["Id", "Cliente", "Importe"],
    [["A-001", "Ana", "125.00"], ["A-002", "Luis", "89.50"]]);

using (var file = File.Create(Path.Combine(outputDirectory, "pedidos.docx")))
    DocumentExporter.WriteDocx(report, file);
using (var file = File.Create(Path.Combine(outputDirectory, "pedidos.xlsx")))
    DocumentExporter.WriteXlsx(report, file);

GlobalFontSettings.FontResolver = new SingleFontResolver(File.ReadAllBytes(args[1]));
using (var file = File.Create(Path.Combine(outputDirectory, "pedidos.pdf")))
    DocumentExporter.WritePdf(report, file, new XFont("LbeExample", 11));

using (var file = File.OpenRead(Path.Combine(outputDirectory, "pedidos.pdf")))
    Console.WriteLine(DocumentExporter.ExtractPdfText(file));
return 0;

sealed class SingleFontResolver(byte[] fontBytes) : IFontResolver
{
    public string DefaultFontName => "LbeExample";

    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        => new("LbeExample");

    public byte[] GetFont(string faceName) => fontBytes;
}
