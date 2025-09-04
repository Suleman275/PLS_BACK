namespace SharedKernel.Models;

internal interface IAuditableEntity {
    DateTime? LastModifiedOn { get; set; }
    Guid? LastModifiedById { get; set; }
    DateTime CreatedOn { get; set; }
    Guid CreatedById { get; set; }
}
