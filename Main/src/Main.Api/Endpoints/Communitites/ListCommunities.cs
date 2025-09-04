using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.Communitites;

public class ListCommunitiesRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }

    [QueryParam]
    public int PageNumber { get; set; } = 1;

    [QueryParam]
    public int PageSize { get; set; } = 10;

    [QueryParam]
    public string? SearchTerm { get; set; }
}

public class CommunityListItem {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int PostCount { get; set; } 
}

public class ListCommunitiesResponse {
    public IEnumerable<CommunityListItem> Communities { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ListCommunitiesRequestValidator : Validator<ListCommunitiesRequest> {
    public ListCommunitiesRequestValidator() {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}

public class ListCommunities(AppDbContext dbContext) : Endpoint<ListCommunitiesRequest, ListCommunitiesResponse> {
    public override void Configure() {
        Get("communities");
        Version(1);
        Permissions(nameof(UserPermission.Communities_Read));
    }

    public override async Task HandleAsync(ListCommunitiesRequest req, CancellationToken ct) {
        var query = dbContext.Communities.AsNoTracking().Where(c => c.DeletedOn == null);

        if (!string.IsNullOrWhiteSpace(req.SearchTerm)) {
            var searchTermLower = req.SearchTerm.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(searchTermLower) ||
                (c.Description != null && c.Description.ToLower().Contains(searchTermLower)));
        }

        var totalCount = await query.CountAsync(ct);

        query = query.OrderBy(c => c.Name).ThenBy(c => c.CreatedOn);

        var communities = await query
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(c => new CommunityListItem {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                PostCount = c.Posts.Count
            })
            .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling((double)totalCount / req.PageSize);

        await SendOkAsync(new ListCommunitiesResponse {
            Communities = communities,
            TotalCount = totalCount,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
            TotalPages = totalPages
        }, ct);
    }
}