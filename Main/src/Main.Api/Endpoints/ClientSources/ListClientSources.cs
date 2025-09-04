using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.ClientSources;

public class ListClientSourcesRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }

    [QueryParam]
    public int PageNumber { get; set; } = 1;

    [QueryParam]
    public int PageSize { get; set; } = 10;

    [QueryParam]
    public string? SearchTerm { get; set; }
}

public class ClientSourceListItem {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class ListClientSourcesResponse {
    public IEnumerable<ClientSourceListItem> ClientSources { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ListClientSourcesRequestValidator : Validator<ListClientSourcesRequest> {
    public ListClientSourcesRequestValidator() {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}

public class ListClientSources(AppDbContext dbContext) : Endpoint<ListClientSourcesRequest, ListClientSourcesResponse> {
    public override void Configure() {
        Get("client-sources");
        Version(1);
        Permissions(nameof(UserPermission.ClientSources_Read));
    }

    public override async Task HandleAsync(ListClientSourcesRequest req, CancellationToken ct) {
        var query = dbContext.ClientSources.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(req.SearchTerm)) {
            var searchTermLower = req.SearchTerm.ToLower();
            query = query.Where(cs =>
                cs.Name.ToLower().Contains(searchTermLower) ||
                (cs.Description != null && cs.Description.ToLower().Contains(searchTermLower)));
        }

        var totalCount = await query.CountAsync(ct);

        var clientSources = await query
            .OrderBy(cs => cs.Name)
            .ThenBy(cs => cs.CreatedOn)
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(cs => new ClientSourceListItem {
                Id = cs.Id,
                Name = cs.Name,
                Description = cs.Description,
            })
            .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling((double)totalCount / req.PageSize);

        await SendOkAsync(new ListClientSourcesResponse {
            ClientSources = clientSources,
            TotalCount = totalCount,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
            TotalPages = totalPages
        }, ct);
    }
}