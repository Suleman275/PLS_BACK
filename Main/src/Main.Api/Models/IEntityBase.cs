namespace SharedKernel.Models;
internal interface IEntityBase : IAuditableEntity, ISoftDeleteableEntity {
    Guid Id { get; set; }
}

