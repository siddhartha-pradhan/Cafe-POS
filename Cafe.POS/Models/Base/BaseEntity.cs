namespace Cafe.POS.Models.Base;

public class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public bool IsActive { get; set; } = true;
    
    public Guid CreatedBy { get; set; }
    
    public DateTime CreatedOn { get; set; } = DateTime.Now;

    public Guid LastModifiedBy { get; set; }
    
    public DateTime LastModifiedOn { get; set; }
}