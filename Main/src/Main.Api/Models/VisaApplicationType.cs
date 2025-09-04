using SharedKernel.Models;

namespace UserManagement.API.Models;

public class VisaApplicationType : EntityBase {
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}
