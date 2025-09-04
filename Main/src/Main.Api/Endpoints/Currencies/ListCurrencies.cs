using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.Currencies;

public class ListCurrenciesRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }

    [QueryParam]
    public int PageNumber { get; set; } = 1;

    [QueryParam]
    public int PageSize { get; set; } = 10;

    [QueryParam]
    public string? SearchTerm { get; set; }
}

public class CurrencyListItem {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!; 
    public string? Description { get; set; }
}

public class ListCurrenciesResponse {
    public IEnumerable<CurrencyListItem> Currencies { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ListCurrenciesRequestValidator : Validator<ListCurrenciesRequest> {
    public ListCurrenciesRequestValidator() {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}

public class ListCurrencies(AppDbContext dbContext) : Endpoint<ListCurrenciesRequest, ListCurrenciesResponse> {
    public override void Configure() {
        Get("currencies");
        Version(1);
        Permissions(nameof(UserPermission.Currencies_Read));
    }

    public override async Task HandleAsync(ListCurrenciesRequest req, CancellationToken ct) {
        var query = dbContext.Currencies.AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.SearchTerm)) {
            var searchTermLower = req.SearchTerm.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(searchTermLower) ||
                c.Code.ToLower().Contains(searchTermLower) || 
                (c.Description != null && c.Description.ToLower().Contains(searchTermLower)));
        }

        var totalCount = await query.CountAsync(ct);

        query = query.OrderBy(c => c.Name).ThenBy(c => c.CreatedOn);

        var currencies = await query
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(c => new CurrencyListItem {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                Description = c.Description,
            })
            .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling((double)totalCount / req.PageSize);

        await SendOkAsync(new ListCurrenciesResponse {
            Currencies = currencies,
            TotalCount = totalCount,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
            TotalPages = totalPages
        }, ct);
    }
}