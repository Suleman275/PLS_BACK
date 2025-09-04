using SharedKernel.Models;

namespace UserManagement.API.Models;

public class Comment : EntityBase {
    public Guid PostId { get; set; }
    public Post Post { get; set; } = default!;

    public string? Content { get; set; }
}
