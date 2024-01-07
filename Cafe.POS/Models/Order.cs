namespace Cafe.POS.Models;

public class Order : BaseEntity
{
    public Guid CustomerId { get; set; }
    
    public Guid CoffeeId { get; set; }
    
    public Guid AddInId { get; set; }
    
    public int CoffeeQuantity { get; set; }

    public decimal AddInQuantity { get; set; }
    
    public decimal TotalPrice { get; set; }
    
    public PaymentMode PaymentMode { get; set; }
}