using SharedKernel.Models;

namespace UserManagement.API.Models;

public class DocumentType : EntityBase {
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}
