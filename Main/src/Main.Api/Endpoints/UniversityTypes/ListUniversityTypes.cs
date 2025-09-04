using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.UniversityTypes;

public class ListUniversityTypesRequest {
    [QueryParam]
    public int PageNumber { get; set; } = 1;

    [QueryParam]
    public int PageSize { get; set; } = 10;

    [QueryParam]
    public string? SearchTerm { get; set; }
}

public class ListUniversityTypesResponse {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class ListUniversityTypesPaginatedResponse {
    public IEnumerable<ListUniversityTypesResponse> Items { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

public class ListUniversityTypesRequestValidator : Validator<ListUniversityTypesRequest> {
    public ListUniversityTypesRequestValidator() {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}

public class ListUniversityTypes(AppDbContext dbContext) : Endpoint<ListUniversityTypesRequest, ListUniversityTypesPaginatedResponse> {
    public override void Configure() {
        Get("university-types");
        Version(1);
        Permissions(nameof(UserPermission.UniversityTypes_Read));
    }

    public override async Task HandleAsync(ListUniversityTypesRequest req, CancellationToken ct) {
        var query = dbContext.UniversityTypes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(req.SearchTerm)) {
            var searchTermLower = req.SearchTerm.ToLower();
            query = query.Where(ut =>
                ut.Name.ToLower().Contains(searchTermLower) ||
                (ut.Description != null && ut.Description.ToLower().Contains(searchTermLower))
            );
        }

        var totalCount = await query.CountAsync(ct);

        var universityTypes = await query
            .OrderBy(ut => ut.Name)
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(ut => new ListUniversityTypesResponse {
                Id = ut.Id,
                Name = ut.Name,
                Description = ut.Description,
            })
            .ToListAsync(ct);

        var paginatedResponse = new ListUniversityTypesPaginatedResponse {
            Items = universityTypes,
            TotalCount = totalCount,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)req.PageSize)
        };

        await SendAsync(paginatedResponse, cancellation: ct);
    }
}