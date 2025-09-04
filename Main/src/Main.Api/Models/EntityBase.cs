namespace SharedKernel.Models;

public class EntityBase : IEntityBase {
    public Guid Id { get; set; } = Guid.CreateVersion7();  
    public DateTime? LastModifiedOn { get; set; }
    public Guid? LastModifiedById { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public Guid CreatedById { get; set; } = Guid.Empty;
    public DateTime? DeletedOn { get; set; }
    public Guid? DeletedById { get; set; }
}