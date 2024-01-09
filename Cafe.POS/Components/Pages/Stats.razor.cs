using Cafe.POS.Models;
using Cafe.POS.Services;
using Microsoft.AspNetCore.Components;

namespace Cafe.POS.Components.Pages;

public partial class Stats
{
    [CascadingParameter] 
    private GlobalState _globalState { get; set; }

    private List<OrderModel> _orderModels { get; set; } = new();
    
    private bool _showStatsDialog { get; set; }
    
    private int _selectedAction { get; set; }

    private int _selectedMonth { get; set; }

    private string _recordErrorMessage { get; set; }

    private string _recordSuccessMessage { get; set; }

    private bool _showMonthsDropdown { get; set; }
    
    private string _dialogTitle { get; set; }
    
    private string _dialogOkLabel { get; set; }
    
    private string _tabFilter = "Coffee";
    
    private readonly string _ordersPath = UtilityService.GetAppOrdersFilePath();
    
    private readonly string _coffeesPath = UtilityService.GetAppCoffeesFilePath();
    
    private readonly string _addInsPath = UtilityService.GetAppAddInsFilePath();
    
    private readonly string _orderAddInsPath = UtilityService.GetAppOrderAddInsFilePath();

    protected override void OnInitialized()
    {
        var coffees = CoffeeService.GetAll(_coffeesPath);

        var orders = OrderService.GetAll(_ordersPath);
        
        _orderModels = [];

        foreach (var coffee in coffees)
        {
            var orderItems = orders.Where(x => x.CoffeeId == coffee.Id);

            var orderList = orderItems as Order[] ?? orderItems.ToArray();

            _orderModels.Add(new OrderModel()
            {
                Id = coffee.Id,
                Name = coffee.Name,
                Price = coffee.Price,
                TotalSales = orderList.Any() ? orderList.Sum(x => x.CoffeeQuantity) : 0,
                LastOrderedDate = orderList.Any() ? orderList.Max(x => x.CreatedOn).ToString("dd-MM-yyyy") : "Not Ordered Yet"
            });
        }
    }
    
    private void TabFilter(string status)
    {
        _tabFilter = status;

        switch (status)
        {
            case "Coffee":
            {
                var coffees = CoffeeService.GetAll(_coffeesPath);

                var orders = OrderService.GetAll(_ordersPath);
                
                _orderModels = [];

                foreach (var coffee in coffees)
                {
                    var orderItems = orders.Where(x => x.CoffeeId == coffee.Id);

                    var orderList = orderItems as Order[] ?? orderItems.ToArray();

                    _orderModels.Add(new OrderModel()
                    {
                        Id = coffee.Id,
                        Name = coffee.Name,
                        Price = coffee.Price,
                        TotalSales = orderList.Any() ? orderList.Sum(x => x.CoffeeQuantity) : 0,
                        LastOrderedDate = orderList.Any() ? orderList.Max(x => x.CreatedOn).ToString("dd-MM-yyyy") : "Not Ordered Yet"
                    });
                }
                
                break;
            }
            case "Add In":
            {
                var addIns = AddInService.GetAll(_addInsPath);
                
                var orderAddIns = OrderAddInService.GetAll(_orderAddInsPath);

                _orderModels = [];

                foreach (var addIn in addIns)
                {
                    var orderItems = orderAddIns.Where(x => x.AddInId == addIn.Id);

                    var orderAddInItems = orderItems as OrderAddIn[] ?? orderItems.ToArray();
                    
                    _orderModels.Add(new OrderModel()
                    {
                        Id = addIn.Id,
                        Name = addIn.Name,
                        Price = addIn.Price,
                        TotalSales = orderAddInItems.Any() ? orderAddInItems.Sum(x => x.AddInQuantity) : 0,
                        LastOrderedDate = orderAddInItems.Any() ? orderAddInItems.Max(x => x.CreatedOn).ToString("dd-MM-yyyy") : "Not Ordered Yet"
                    });
                }
                
                break;
            }
        }
    }
    
    private void OpenReportDialog()
    {
        _dialogTitle = "Report Generation";

        _dialogOkLabel = "Select";

        _showStatsDialog = true;

        _showMonthsDropdown = false;

        _selectedMonth = 0;
        
        _recordErrorMessage = "";
        
        _recordSuccessMessage = "";

        _selectedAction = 0;
    }

    private void FrequencyChangeHandler(ChangeEventArgs e)
    {
        _selectedAction = Convert.ToInt32(e.Value.ToString());

        _showMonthsDropdown = _selectedAction == 2;
    }
    
    private void OnDownloadReport(bool isClosed)
    {
        if (isClosed)
        {
            _showStatsDialog = false;
        }
        else
        {
            try
            {
                _recordErrorMessage = "";

                switch (_selectedAction)
                {
                    case 0:
                        throw new Exception("Select a valid action before submitting your request.");
                    case 1:
                        break;
                    case 2 when _selectedMonth == 0:
                        throw new Exception("Select a valid action before submitting your request.");
                    case 2:
                        break;
                }

                GeneratePdf();
                
                _recordSuccessMessage = "Your report has been successfully generated, please check your downloads folder.";
            }
            catch (Exception e)
            {
                _recordErrorMessage = e.Message;

                Console.WriteLine(e.Message);
            }
        }
    }
    
    private void GeneratePdf()
    {
        if (_selectedAction == 1)
        {
            var coffees = CoffeeService.GetAll(_coffeesPath);
            var addIns = AddInService.GetAll(_addInsPath);
            var orderAddIns = OrderAddInService.GetAll(_orderAddInsPath);
            var orders = OrderService.GetAll(_ordersPath).Where(x => x.CreatedOn.Date == DateTime.Today.Date);

            var coffeeModel = coffees
                .Select(coffee => new OrderModel
                {
                    Id = coffee.Id,
                    Name = coffee.Name,
                    Price = coffee.Price,
                    TotalSales = orders.Where(x => x.CoffeeId == coffee.Id).Sum(x => x.CoffeeQuantity),
                    LastOrderedDate = orders.Where(x => x.CoffeeId == coffee.Id)
                        .Select(x => x.CreatedOn)
                        .DefaultIfEmpty(DateTime.MinValue)
                        .Max()
                        .ToString("dd-MM-yyyy")
                })
                .OrderByDescending(x => x.TotalSales)
                .Take(5);

            var addInModel = addIns
                .Select(addIn => new OrderModel
                {
                    Id = addIn.Id,
                    Name = addIn.Name,
                    Price = addIn.Price,
                    TotalSales = orderAddIns.Where(x => x.AddInId == addIn.Id).Sum(x => x.AddInQuantity),
                    LastOrderedDate = orderAddIns.Where(x => x.AddInId == addIn.Id)
                        .Select(x => x.CreatedOn)
                        .DefaultIfEmpty(DateTime.MinValue)
                        .Max()
                        .ToString("dd-MM-yyyy")
                })
                .OrderByDescending(x => x.TotalSales)
                .Take(5);

            ReportService.GeneratePdfReport(_jsRuntime, "report.pdf", coffeeModel, addInModel);
        }
        else
        {
            var coffees = CoffeeService.GetAll(_coffeesPath);
            var addIns = AddInService.GetAll(_addInsPath);
            var orderAddIns = OrderAddInService.GetAll(_orderAddInsPath);
            var orders = OrderService.GetAll(_ordersPath).Where(x => x.CreatedOn.Month == _selectedMonth);

            var coffeeModel = coffees
                .Select(coffee => new OrderModel
                {
                    Id = coffee.Id,
                    Name = coffee.Name,
                    Price = coffee.Price,
                    TotalSales = orders.Where(x => x.CoffeeId == coffee.Id).Sum(x => x.CoffeeQuantity),
                    LastOrderedDate = orders.Where(x => x.CoffeeId == coffee.Id)
                        .Select(x => x.CreatedOn)
                        .DefaultIfEmpty(DateTime.MinValue)
                        .Max()
                        .ToString("dd-MM-yyyy")
                })
                .OrderByDescending(x => x.TotalSales)
                .Take(5);

            var addInModel = addIns
                .Select(addIn => new OrderModel
                {
                    Id = addIn.Id,
                    Name = addIn.Name,
                    Price = addIn.Price,
                    TotalSales = orderAddIns.Where(x => x.AddInId == addIn.Id).Sum(x => x.AddInQuantity),
                    LastOrderedDate = orderAddIns.Where(x => x.AddInId == addIn.Id)
                        .Select(x => x.CreatedOn)
                        .DefaultIfEmpty(DateTime.MinValue)
                        .Max()
                        .ToString("dd-MM-yyyy")
                })
                .OrderByDescending(x => x.TotalSales)
                .Take(5);

            ReportService.GeneratePdfReport(_jsRuntime, "report.pdf", coffeeModel, addInModel);
        }
    }
}

public class OrderModel
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }

    public decimal Price { get; set; }
    
    public int TotalSales { get; set; }

    public string LastOrderedDate { get; set; }
}