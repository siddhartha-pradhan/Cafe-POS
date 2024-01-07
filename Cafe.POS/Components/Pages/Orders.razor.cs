using Cafe.POS.Models;
using Cafe.POS.Services;
using Microsoft.AspNetCore.Components;

namespace Cafe.POS.Components.Pages;

public partial class Orders
{
    [CascadingParameter] 
    private GlobalState _globalState { get; set; }

    private List<Order> _orders { get; set; }

    private List<Customer> _customers { get; set; }

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

    private readonly string _customersPath = UtilityService.GetAppCustomersFilePath();
    
    private readonly string _coffeesPath = UtilityService.GetAppCoffeesFilePath();
    
    private readonly string _addInsPath = UtilityService.GetAppAddInsFilePath();

    protected override void OnInitialized()
    {
        _orders = OrderService.GetAll(_ordersPath);
        _customers = CustomerService.GetAll(_customersPath);
        _coffees = CoffeeService.GetAll(_coffeesPath);
        _addIns = AddInService.GetAll(_addInsPath);
    }

    private void OpenAddUserDialog()
    {
        _dialogTitle = "Add a new order";

        _dialogOkLabel = "Add";

        _orderModel = new Order();

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
                    AddInId = _orderModel.AddInId,
                    CoffeeQuantity = _orderModel.CoffeeQuantity,
                    AddInQuantity = _orderModel.AddInQuantity,
                    TotalPrice = _payableAmount,
                    PaymentMode = _orderModel.PaymentMode,
                    IsActive = true,
                    CreatedBy = _globalState.User.Id,
                };

                _orders = OrderService.Create(order);
                
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
        var coffeeQuantity = int.Parse(e.Value.ToString());

        var coffeePrice = _coffees.FirstOrDefault(x => x.Id == _orderModel.CoffeeId)?.Price ?? 0;

        var addInPrice = _addIns.FirstOrDefault(x => x.Id == _orderModel.AddInId)?.Price ?? 0;

        var addInQuantity = _orderModel?.AddInQuantity ?? 0;

        var coffeeAmount = coffeePrice * coffeeQuantity;

        var addInAmount = addInPrice * addInQuantity;
        
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
    
    private void OnAddInQuantityChange(ChangeEventArgs e)
    {
        var addInQuantity = int.Parse(e.Value.ToString());

        var coffeePrice = _coffees.FirstOrDefault(x => x.Id == _orderModel.CoffeeId)?.Price ?? 0;

        var addInPrice = _addIns.FirstOrDefault(x => x.Id == _orderModel.AddInId)?.Price ?? 0;

        var coffeeQuantity = _orderModel?.CoffeeQuantity ?? 0;

        var coffeeAmount = coffeePrice * coffeeQuantity;

        var addInAmount = addInPrice * addInQuantity;
        
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
}