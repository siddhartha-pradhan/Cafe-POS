using Cafe.POS.Models;
using Cafe.POS.Services;
using Microsoft.AspNetCore.Components;

namespace Cafe.POS.Components.Pages;

public partial class Orders
{
    [CascadingParameter] 
    private GlobalState _globalState { get; set; }

    private List<Order> _orders { get; set; }

    private List<OrderAddIn> _orderAddIns { get; set; }

    private List<Customer> _customers { get; set; }

    private List<OrderAddIn> _orderAddInModel { get; set; } =
    [
        new OrderAddIn()
        {
            AddInId = Guid.Empty,
            AddInQuantity = 0
        }
    ];

    private List<Coffee> _coffees { get; set; }
    
    private List<AddIn> _addIns { get; set; }
    
    private bool _showAddOrderDialog { get; set; }

    private bool? _isRegularCustomer { get; set; }

    private bool? _isComplimentaryCoffee { get; set; }
    
    private string _addOrderErrorMessage { get; set; }
    
    private Order? _orderModel { get; set; }
    
    private string _dialogTitle { get; set; }
    
    private string _dialogOkLabel { get; set; }
    
    private string _tabFilter = "All";
    
    private string _sortDirection = "ascending";

    private decimal _actualAmount;
    
    private decimal _payableAmount;
    
    private readonly string _ordersPath = UtilityService.GetAppOrdersFilePath();

    private readonly string _orderAddInsPath = UtilityService.GetAppOrderAddInsFilePath();

    private readonly string _customersPath = UtilityService.GetAppCustomersFilePath();
    
    private readonly string _coffeesPath = UtilityService.GetAppCoffeesFilePath();
    
    private readonly string _addInsPath = UtilityService.GetAppAddInsFilePath();

    protected override void OnInitialized()
    {
        _orders = OrderService.GetAll(_ordersPath);
        _customers = CustomerService.GetAll(_customersPath);
        _coffees = CoffeeService.GetAll(_coffeesPath).Where(x => x.IsActive).ToList();
        _addIns = AddInService.GetAll(_addInsPath).Where(x => x.IsActive).ToList();
        _orderAddIns = OrderAddInService.GetAll(_orderAddInsPath);
    }

    private void OpenAddUserDialog()
    {
        _dialogTitle = "Add a new order";

        _dialogOkLabel = "Add";

        _orderModel = new Order();

        _orderAddInModel = 
        [
            new OrderAddIn()
            {
                AddInId = Guid.Empty,
                AddInQuantity = 0
            }
        ];
        
        _showAddOrderDialog = true;
    }

    private void OnAddOrderDialogClose(bool isClosed)
    {
        if (isClosed)
        {
            _showAddOrderDialog = false;
        }
        else
        {
            try
            {
                _addOrderErrorMessage = "";

                var order = new Order()
                {
                    CoffeeId = _orderModel.CoffeeId,
                    CustomerId = _orderModel.CustomerId,
                    CoffeeQuantity = _orderModel.CoffeeQuantity,
                    TotalPrice = _payableAmount,
                    PaymentMode = _orderModel.PaymentMode,
                    IsActive = true,
                    CreatedBy = _globalState.User.Id,
                };

                var orderAddIns = _orderAddInModel.Select(x => new OrderAddIn()
                {
                    OrderId = order.Id,
                    AddInId = x.AddInId,
                    AddInQuantity = x.AddInQuantity,
                    IsActive = true,
                    CreatedBy = _globalState.User.Id,
                }).ToList();
                
                _orders = OrderService.Create(order);

                _orderAddIns = OrderAddInService.Create(orderAddIns);

                CustomerService.UpdateOrderCount(_orderModel.CustomerId);
                
                _showAddOrderDialog = false;
            }
            catch (Exception e)
            {
                _addOrderErrorMessage = e.Message;

                Console.WriteLine(e.Message);
            }
        }
    }

    private void OnCustomerSelection(ChangeEventArgs e)
    {
        var customerId = Guid.Parse(e.Value.ToString());

        var isRegularCustomer = CustomerService.IsRegularCustomer(customerId);

        var isCoffeeComplimentary = CustomerService.IsAvailableForComplimentaryCoffee(customerId);
        
        _isRegularCustomer = isRegularCustomer;
        
        _isComplimentaryCoffee = isCoffeeComplimentary;

        _orderModel ??= new Order();

        _orderModel.CustomerId = customerId;
    }

    private void OnCoffeeQuantityChange(ChangeEventArgs e)
    {
        if (e.Value == null) return;

        var stringValue = e.Value.ToString();
            
        if (string.IsNullOrWhiteSpace(stringValue))
        {
            e.Value = 0;
        }
        else
        {
            e.Value = int.TryParse(stringValue, out var intValue) ? intValue : 0;
        }
        
        var coffeeQuantity = int.Parse(e.Value.ToString());

        var coffeePrice = _coffees.FirstOrDefault(x => x.Id == _orderModel.CoffeeId)?.Price ?? 0;

        var addInAmount = (from orderAddIn in _orderAddInModel 
            let addInPrice = AddInService.GetAll(_addInsPath).FirstOrDefault(x => x.Id == orderAddIn.AddInId)?.Price ?? 0 
            let addInQuantity = orderAddIn.AddInQuantity 
            select addInPrice * addInQuantity).Sum();

        var coffeeAmount = coffeePrice * coffeeQuantity;
        
        _actualAmount = coffeeAmount + addInAmount;

        if (_isRegularCustomer.HasValue && _isRegularCustomer.Value)
        {
            _payableAmount = _actualAmount - (_actualAmount * 0.15m);
        }
        else if (_isComplimentaryCoffee.HasValue && _isComplimentaryCoffee.Value)
        {
            _payableAmount = _actualAmount - coffeeAmount;
        }
        else
        {
            _payableAmount = _actualAmount;
        }
    }
    
    private void OnAddInQuantityChange(ChangeEventArgs e, int index)
    {
        if (e.Value == null) return;
        
        var stringValue = e.Value.ToString();
            
        if (string.IsNullOrWhiteSpace(stringValue))
        {
            e.Value = 0;
        }
        else
        {
            e.Value = int.TryParse(stringValue, out var intValue) ? intValue : 0;
        }
        
        var addInQuantityCount = int.Parse(e.Value.ToString());

        _orderAddInModel[index].AddInQuantity = addInQuantityCount;
        
        var coffeePrice = _coffees.FirstOrDefault(x => x.Id == _orderModel.CoffeeId)?.Price ?? 0;

        var coffeeQuantity = _orderModel?.CoffeeQuantity ?? 0;

        var coffeeAmount = coffeePrice * coffeeQuantity;

        var addInAmount = (from orderAddIn in _orderAddInModel 
            let addInPrice = AddInService.GetAll(_addInsPath).FirstOrDefault(x => x.Id == orderAddIn.AddInId)?.Price ?? 0 
            let addInQuantity = orderAddIn.AddInQuantity 
            select addInPrice * addInQuantity).Sum();
        
        _actualAmount = coffeeAmount + addInAmount;

        if (_isRegularCustomer.HasValue && _isRegularCustomer.Value)
        {
            _payableAmount = _actualAmount - (_actualAmount * 0.15m);
        }
        else if (_isComplimentaryCoffee.HasValue && _isComplimentaryCoffee.Value)
        {
            _payableAmount = _actualAmount - coffeeAmount;
        }
        else
        {
            _payableAmount = _actualAmount;
        }
    }

    private void AddAddIn()
    {
        _orderAddInModel.Add(new OrderAddIn()
        {
            AddInId = Guid.Empty,
            AddInQuantity = 0
        });
    }
}