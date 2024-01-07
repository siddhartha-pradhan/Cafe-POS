namespace Cafe.POS.Models;

public class Coffee : BaseEntity
{
    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;
    
    public decimal Price { get; set; }
}