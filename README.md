# Lbe.Documents

[![CI](https://github.com/lbe2014/lbe-documents/actions/workflows/ci.yml/badge.svg)](https://github.com/lbe2014/lbe-documents/actions/workflows/ci.yml)

Biblioteca pequeña de .NET 10 para generar el mismo reporte tabular en **DOCX**, **XLSX** y **PDF**, y extraer la capa de texto de un PDF. Usa [Open XML SDK](https://github.com/dotnet/Open-XML-SDK), [ClosedXML](https://github.com/ClosedXML/ClosedXML), [PDFsharp](https://github.com/empira/PDFsharp) y [PdfPig](https://github.com/UglyToad/PdfPig).

Clona el repositorio y añade una referencia al proyecto de la biblioteca desde tu aplicación:

```powershell
dotnet add <ruta-a-tu-aplicacion.csproj> reference src/Lbe.Documents/Lbe.Documents.csproj
```

La versión `0.1.0` también se puede empaquetar localmente; todavía no se ha publicado en NuGet.

## Inicio rápido

```csharp
using Lbe.Documents;

var report = new TabularReport(
    "Pedidos",
    ["Id", "Cliente", "Importe"],
    [["A-001", "Ana", "125.00"]]);

using var word = File.Create("pedidos.docx");
DocumentExporter.WriteDocx(report, word);

using var excel = File.Create("pedidos.xlsx");
DocumentExporter.WriteXlsx(report, excel);
```

El [ejemplo completo](examples/Lbe.Documents.Example/Program.cs) también crea un PDF y vuelve a leer su texto. Para el PDF debes suministrar una fuente TTF y configurar el `IFontResolver` de PDFsharp antes de crear `XFont`; así funciona también en Linux sin depender de fuentes instaladas en el servidor. No se incluye un archivo de fuente en el repositorio.

```powershell
dotnet run --project examples/Lbe.Documents.Example -- ./salida C:/Windows/Fonts/arial.ttf
```

## Alcance y límites

- Los valores de XLSX se escriben como texto para conservar IDs y ceros iniciales. Si necesitas fórmulas o tipos numéricos, trabaja directamente con ClosedXML.
- El PDF usa una tabla simple; el texto demasiado ancho se acorta visualmente con puntos suspensivos. DOCX y XLSX conservan el valor completo.
- `ExtractPdfText` lee PDFs con capa de texto. Para escaneos hace falta OCR, que aquí no está incluido.
- Open XML SDK no convierte DOCX a PDF ni ofrece un motor de maquetación de Word.
- La biblioteca no automatiza Microsoft Office y no requiere instalarlo en el servidor.

## Desarrollo

```powershell
dotnet test tests/Lbe.Documents.Tests/Lbe.Documents.Tests.csproj
dotnet pack src/Lbe.Documents/Lbe.Documents.csproj -c Release
```

El código es MIT. Revisa también las licencias de las dependencias y de la fuente TTF que uses en tu aplicación.
