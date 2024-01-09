using System.Globalization;
using Cafe.POS.Components.Pages;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.JSInterop;
using Element = iTextSharp.text.Element;

namespace Cafe.POS.Services;

public static class ReportService
{
    public static void GeneratePdfReport(IJSRuntime jsRuntime, string fileName, IEnumerable<OrderModel> orderModels, IEnumerable<OrderModel> addInModels)
    {
        jsRuntime.InvokeAsync<object>("downloadPdfReport", fileName, Convert.ToBase64String(PdfReport(orderModels, addInModels)));
        jsRuntime.InvokeAsync<object>("openPdfFile", fileName);
    }
    
    private static byte[] PdfReport(IEnumerable<OrderModel> coffeeModels, IEnumerable<OrderModel> addInModels)
    {
        var document = new Document(PageSize.A4, 20, 20, 40, 40);
        
        var output = new MemoryStream();
        
        var writer = PdfWriter.GetInstance(document, output);
        
        writer.CloseStream = false;
        
        writer.PageEvent = new PdfPageEventHelper();

        document.Open();

        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
        
        var title = new Paragraph("Cafe.POS Revenue Report", titleFont)
        {
            Alignment = Element.ALIGN_CENTER
        };
        
        document.Add(title);

        var dateFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
        
        var currentDate = DateTime.Now.ToString("dddd, dd MMMM yyyy", CultureInfo.InvariantCulture);
        
        var dateParagraph = new Paragraph($"Report generated on: {currentDate}", dateFont)
        {
            Alignment = Element.ALIGN_CENTER,
            SpacingAfter = 20
        };
        document.Add(dateParagraph);

        var coffeeTable = new PdfPTable(4)
        {
            WidthPercentage = 100
        };
        
        coffeeTable.SetWidths(new[] { 2, 1, 1, 2 });

        coffeeTable.AddCell(CreateCell("Coffee Name", true));
        coffeeTable.AddCell(CreateCell("Price", true));
        coffeeTable.AddCell(CreateCell("Total Sales", true));
        coffeeTable.AddCell(CreateCell("Last Ordered Date", true));

        foreach (var item in coffeeModels)
        {
            coffeeTable.AddCell(CreateCell(item.Name));
            coffeeTable.AddCell(CreateCell($"Rs {item.Price.ToString("N2", CultureInfo.CreateSpecificCulture("ne-NP"))}"));
            coffeeTable.AddCell(CreateCell(item.TotalSales.ToString()));
            coffeeTable.AddCell(CreateCell(item.LastOrderedDate));
        }

        document.Add(coffeeTable);

        var addInTable = new PdfPTable(4)
        {
            WidthPercentage = 100
        };
        
        addInTable.SetWidths(new[] { 2, 1, 1, 2 });

        addInTable.AddCell(CreateCell("Add In Title", true));
        addInTable.AddCell(CreateCell("Price", true));
        addInTable.AddCell(CreateCell("Total Sales", true));
        addInTable.AddCell(CreateCell("Last Ordered Date", true));

        foreach (var item in addInModels)
        {
            addInTable.AddCell(CreateCell(item.Name));
            addInTable.AddCell(CreateCell($"Rs {item.Price.ToString("N2", CultureInfo.CreateSpecificCulture("ne-NP"))}"));
            addInTable.AddCell(CreateCell(item.TotalSales.ToString()));
            addInTable.AddCell(CreateCell(item.LastOrderedDate));
        }

        document.Add(addInTable);

        document.Close();
        
        return output.ToArray();
    }

    private static PdfPCell CreateCell(string content, bool isHeader = false)
    {
        var cell = new PdfPCell(new Phrase(content, isHeader ? FontFactory.GetFont(FontFactory.HELVETICA_BOLD) : FontFactory.GetFont(FontFactory.HELVETICA)))
        {
            Padding = 8,
            BackgroundColor = isHeader ? new BaseColor(200, 200, 200) : BaseColor.WHITE
        };
        
        return cell;
    }

}