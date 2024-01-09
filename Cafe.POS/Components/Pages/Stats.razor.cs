using Bislerium.Services;
using Cafe.POS.Models;
using Cafe.POS.Services;
using Microsoft.AspNetCore.Components;

namespace Cafe.POS.Components.Pages;

public partial class Stats
{
    [CascadingParameter] 
    private GlobalState _globalState { get; set; }

    private List<OrderModel> _orderModels { get; set; } = new();
    
    private string _tabFilter = "Coffee";
    
    private readonly string _ordersPath = UtilityService.GetAppOrdersFilePath();
    
    private readonly string _coffeesPath = UtilityService.GetAppCoffeesFilePath();
    
    private readonly string _addInsPath = UtilityService.GetAppAddInsFilePath();

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
                TotalSales = orderList.Any() ? orderList.Count() : 0,
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
                        TotalSales = orderList.Any() ? orderList.Count() : 0,
                        LastOrderedDate = orderList.Any() ? orderList.Max(x => x.CreatedOn).ToString("dd-MM-yyyy") : "Not Ordered Yet"
                    });
                }
                
                break;
            }
            case "Add In":
            {
                var addIns = AddInService.GetAll(_addInsPath);

                var orders = OrderService.GetAll(_ordersPath);

                _orderModels = [];

                foreach (var addIn in addIns)
                {
                    var orderItems = orders.Where(x => x.AddInId == addIn.Id);

                    var orderList = orderItems as Order[] ?? orderItems.ToArray();
            
                    _orderModels.Add(new OrderModel()
                    {
                        Id = addIn.Id,
                        Name = addIn.Name,
                        Price = addIn.Price,
                        TotalSales = orderList.Any() ? orderList.Count() : 0,
                        LastOrderedDate = orderList.Any() ? orderList.Max(x => x.CreatedOn).ToString("dd-MM-yyyy") : "Not Ordered Yet"
                    });
                }
                
                break;
            }
        }
    }

    private void GeneratePdf(string fileName)
    {
        ReportService.GeneratePdfReport(_jsRuntime, fileName);
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