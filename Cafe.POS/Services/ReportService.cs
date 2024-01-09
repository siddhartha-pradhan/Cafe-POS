using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.JSInterop;

namespace Bislerium.Services;

public static class ReportService
{
    public static void GeneratePdfReport(IJSRuntime jsRuntime, string fileName = "report.pdf")
    {
        jsRuntime.InvokeAsync<object>("downloadPdfReport", fileName, Convert.ToBase64String(PdfReport()));
        jsRuntime.InvokeAsync<object>("openPdfFile", fileName);
    }
    
    private static byte[] PdfReport()
    {
        var document = new Document(
            PageSize.A4, 50, 50, 25, 25);
        
        var font = FontFactory.GetFont("Arial", 16, BaseColor.BLACK);
        
        document.AddTitle("Bislerium Revenue Report");
        
        
        var output = new MemoryStream();
        var writer = PdfWriter.GetInstance(document, output);
        
        document.Open();
        document.Add(new Paragraph("Hello World"));
        document.Close();
        return output.ToArray();
    }
}