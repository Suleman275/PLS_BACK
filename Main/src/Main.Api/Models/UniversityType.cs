using SharedKernel.Models;

namespace UserManagement.API.Models;

public class UniversityType : EntityBase {
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}
