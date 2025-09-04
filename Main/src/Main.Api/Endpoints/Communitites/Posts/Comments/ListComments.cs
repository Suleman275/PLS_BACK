using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.Communitites.Posts.Comments;

public class ListCommentsRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    [FromRoute]
    public Guid CommunityId { get; set; }
    [FromRoute]
    public Guid PostId { get; set; }
    [QueryParam]
    public int PageNumber { get; set; } = 1;
    [QueryParam]
    public int PageSize { get; set; } = 10;
}

public class CommentListItem {
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public string? Content { get; set; }
    public Guid CreatedById { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class ListCommentsResponse {
    public IEnumerable<CommentListItem> Comments { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ListCommentsRequestValidator : Validator<ListCommentsRequest> {
    public ListCommentsRequestValidator() {
        RuleFor(x => x.CommunityId)
            .NotEmpty().WithMessage("Community ID is required.");
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("Post ID is required.");
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}

public class ListComments(AppDbContext dbContext) : Endpoint<ListCommentsRequest, ListCommentsResponse> {
    public override void Configure() {
        Get("communities/{communityId}/posts/{postId}/comments");
        Version(1);
        Permissions(nameof(UserPermission.Comments_Read));
    }

    public override async Task HandleAsync(ListCommentsRequest req, CancellationToken ct) {
        // Verify that the parent post exists and is not soft-deleted
        var postExists = await dbContext.Posts
            .AnyAsync(p => p.Id == req.PostId && p.CommunityId == req.CommunityId && p.DeletedOn == null, ct);

        if (!postExists) {
            await SendNotFoundAsync(ct);
            return;
        }

        var query = dbContext.Comments
            .AsNoTracking()
            .Where(c => c.PostId == req.PostId && c.DeletedOn == null);

        var totalCount = await query.CountAsync(ct);

        query = query.OrderBy(c => c.CreatedOn);

        var comments = await query
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(c => new CommentListItem {
                Id = c.Id,
                PostId = c.PostId,
                Content = c.Content,
                CreatedById = c.CreatedById,
                CreatedOn = c.CreatedOn,
            })
            .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling((double)totalCount / req.PageSize);

        await SendOkAsync(new ListCommentsResponse {
            Comments = comments,
            TotalCount = totalCount,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
            TotalPages = totalPages
        }, ct);
    }
}