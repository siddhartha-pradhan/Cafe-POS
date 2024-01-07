namespace Cafe.POS.Models;

public class Customer : BaseEntity
{
    public string FullName { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;
    
    public int Orders { get; set; }
}