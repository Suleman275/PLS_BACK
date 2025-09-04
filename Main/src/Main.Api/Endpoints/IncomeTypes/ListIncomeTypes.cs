using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.IncomeTypes;

public class ListIncomeTypesRequest {
    [QueryParam]
    public int PageNumber { get; set; } = 1;

    [QueryParam]
    public int PageSize { get; set; } = 10;

    [QueryParam]
    public string? SearchTerm { get; set; }
}

public class IncomeTypeListItem {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class ListIncomeTypesResponse {
    public IEnumerable<IncomeTypeListItem> IncomeTypes { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ListIncomeTypesRequestValidator : Validator<ListIncomeTypesRequest> {
    public ListIncomeTypesRequestValidator() {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}

public class ListIncomeTypes(AppDbContext dbContext) : Endpoint<ListIncomeTypesRequest, ListIncomeTypesResponse> {
    public override void Configure() {
        Get("income-types");
        Version(1);
        Permissions(nameof(UserPermission.IncomeTypes_Read));
    }

    public override async Task HandleAsync(ListIncomeTypesRequest req, CancellationToken ct) {
        var query = dbContext.IncomeTypes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(req.SearchTerm)) {
            var searchTermLower = req.SearchTerm.ToLower();
            query = query.Where(it =>
                it.Name.ToLower().Contains(searchTermLower) ||
                (it.Description != null && it.Description.ToLower().Contains(searchTermLower)));
        }

        var totalCount = await query.CountAsync(ct);

        query = query.OrderBy(it => it.Name).ThenBy(it => it.CreatedOn);

        var incomeTypes = await query
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(it => new IncomeTypeListItem {
                Id = it.Id,
                Name = it.Name,
                Description = it.Description,
            })
            .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling((double)totalCount / req.PageSize);

        await SendOkAsync(new ListIncomeTypesResponse {
            IncomeTypes = incomeTypes,
            TotalCount = totalCount,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
            TotalPages = totalPages
        }, ct);
    }
}