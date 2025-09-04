using SharedKernel.Models;

namespace UserManagement.API.Models;

public class Community : EntityBase {
    public string Name { get; set; } = default!;
    public string? Description { get; set; }

    public ICollection<Post> Posts { get; set; } = [];
}
