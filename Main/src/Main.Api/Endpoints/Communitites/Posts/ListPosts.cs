namespace UserManagement.API.Endpoints.Communitites.Posts;

using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

public class ListPostsRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    [FromRoute]
    public Guid CommunityId { get; set; }
    [QueryParam]
    public int PageNumber { get; set; } = 1;
    [QueryParam]
    public int PageSize { get; set; } = 10;
    [QueryParam]
    public string? SearchTerm { get; set; }
}

public class PostListItem {
    public Guid Id { get; set; }
    public Guid CommunityId { get; set; }
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public int CommentCount { get; set; }
}

public class ListPostsResponse {
    public IEnumerable<PostListItem> Posts { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ListPostsRequestValidator : Validator<ListPostsRequest> {
    public ListPostsRequestValidator() {
        RuleFor(x => x.CommunityId)
            .NotEmpty().WithMessage("Community ID is required.");
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}

public class ListPosts(AppDbContext dbContext) : Endpoint<ListPostsRequest, ListPostsResponse> {
    public override void Configure() {
        Get("communities/{communityId}/posts");
        Version(1);
        Permissions(nameof(UserPermission.Posts_Read));
    }

    public override async Task HandleAsync(ListPostsRequest req, CancellationToken ct) {
        var communityExists = await dbContext.Communities
            .AnyAsync(c => c.Id == req.CommunityId && c.DeletedOn == null, ct);

        if (!communityExists) {
            await SendNotFoundAsync(ct);
            return;
        }

        var query = dbContext.Posts
            .AsNoTracking()
            .Where(p => p.CommunityId == req.CommunityId && p.DeletedOn == null);

        if (!string.IsNullOrWhiteSpace(req.SearchTerm)) {
            var searchTermLower = req.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Title.ToLower().Contains(searchTermLower) ||
                p.Content.ToLower().Contains(searchTermLower));
        }

        var totalCount = await query.CountAsync(ct);

        query = query.OrderBy(p => p.CreatedOn);

        var posts = await query
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(p => new PostListItem {
                Id = p.Id,
                CommunityId = p.CommunityId,
                Title = p.Title,
                Content = p.Content,
                CommentCount = p.Comments.Count
            })
            .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling((double)totalCount / req.PageSize);

        await SendOkAsync(new ListPostsResponse {
            Posts = posts,
            TotalCount = totalCount,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
            TotalPages = totalPages
        }, ct);
    }
}