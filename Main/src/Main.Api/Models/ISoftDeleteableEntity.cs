namespace SharedKernel.Models;

internal interface ISoftDeleteableEntity {
    DateTime? DeletedOn { get; set; }
    Guid? DeletedById { get; set; }
}
