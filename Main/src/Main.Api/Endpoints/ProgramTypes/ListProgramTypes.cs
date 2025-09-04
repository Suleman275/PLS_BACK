using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.ProgramTypes;

public class ListProgramTypesRequest {
    [QueryParam]
    public int PageNumber { get; set; } = 1;

    [QueryParam]
    public int PageSize { get; set; } = 10;

    [QueryParam]
    public string? SearchTerm { get; set; }
}

public class ListProgramTypesResponse {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class ListProgramTypesPaginatedResponse {
    public IEnumerable<ListProgramTypesResponse> Items { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

public class ListProgramTypesRequestValidator : Validator<ListProgramTypesRequest> {
    public ListProgramTypesRequestValidator() {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}

public class ListProgramTypes(AppDbContext dbContext) : Endpoint<ListProgramTypesRequest, ListProgramTypesPaginatedResponse> {
    public override void Configure() {
        Get("program-types");
        Version(1);
        Permissions(nameof(UserPermission.ProgramTypes_Read));
    }

    public override async Task HandleAsync(ListProgramTypesRequest req, CancellationToken ct) {
        var query = dbContext.ProgramTypes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(req.SearchTerm)) {
            var searchTermLower = req.SearchTerm.ToLower();
            query = query.Where(pt =>
                pt.Name.ToLower().Contains(searchTermLower) ||
                (pt.Description != null && pt.Description.ToLower().Contains(searchTermLower))
            );
        }

        var totalCount = await query.CountAsync(ct);

        var programTypes = await query
            .OrderBy(pt => pt.Name)
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(pt => new ListProgramTypesResponse {
                Id = pt.Id,
                Name = pt.Name,
                Description = pt.Description,
            })
            .ToListAsync(ct);

        var paginatedResponse = new ListProgramTypesPaginatedResponse {
            Items = programTypes,
            TotalCount = totalCount,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)req.PageSize)
        };

        await SendAsync(paginatedResponse, cancellation: ct);
    }
}