using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.ExpenseTypes;

public class ListExpenseTypesRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }

    [QueryParam]
    public int PageNumber { get; set; } = 1;

    [QueryParam]
    public int PageSize { get; set; } = 10;

    [QueryParam]
    public string? SearchTerm { get; set; }
}

public class ExpenseTypeListItem {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class ListExpenseTypesResponse {
    public IEnumerable<ExpenseTypeListItem> ExpenseTypes { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ListExpenseTypesRequestValidator : Validator<ListExpenseTypesRequest> {
    public ListExpenseTypesRequestValidator() {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}

public class ListExpenseTypes(AppDbContext dbContext) : Endpoint<ListExpenseTypesRequest, ListExpenseTypesResponse> {
    public override void Configure() {
        Get("expense-types");
        Version(1);
        Permissions(nameof(UserPermission.ExpenseTypes_Read)); 
    }

    public override async Task HandleAsync(ListExpenseTypesRequest req, CancellationToken ct) {
        var query = dbContext.ExpenseTypes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.SearchTerm)) {
            var searchTermLower = req.SearchTerm.ToLower();
            query = query.Where(et =>
                et.Name.ToLower().Contains(searchTermLower) ||
                (et.Description != null && et.Description.ToLower().Contains(searchTermLower)));
        }

        var totalCount = await query.CountAsync(ct);

        query = query.OrderBy(et => et.Name).ThenBy(et => et.CreatedOn);

        var expenseTypes = await query
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(et => new ExpenseTypeListItem {
                Id = et.Id,
                Name = et.Name,
                Description = et.Description,
            })
            .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling((double)totalCount / req.PageSize);

        await SendOkAsync(new ListExpenseTypesResponse {
            ExpenseTypes = expenseTypes,
            TotalCount = totalCount,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
            TotalPages = totalPages
        }, ct);
    }
}
