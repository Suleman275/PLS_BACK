using SharedKernel.Models;

namespace UserManagement.API.Models;

public class IncomeType : EntityBase {
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}