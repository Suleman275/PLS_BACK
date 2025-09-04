using SharedKernel.Models;

namespace UserManagement.API.Models;
public class ClientSource : EntityBase {
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}
