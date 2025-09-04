using SharedKernel.Models;
using UserManagement.API.Enums;

namespace UserManagement.API.Models;

public class Document : EntityBase {
    public string FileName { get; set; } = default!;
    public string S3Key { get; set; } = default!;
    public long FileSize { get; set; }
    public DocumentStatus DocumentStatus { get; set; } = DocumentStatus.UnderReview;
    public string? Description { get; set; }

    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = default!;

    public Guid DocumentTypeId { get; set; }
    public DocumentType DocumentType { get; set; } = default!;
}