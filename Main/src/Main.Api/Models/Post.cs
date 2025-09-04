using SharedKernel.Models;

namespace UserManagement.API.Models;

public class Post : EntityBase {
    public Guid CommunityId { get; set; }
    public Community Community { get; set; } = default;

    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;

    public ICollection<Comment> Comments { get; set; } = [];
}
