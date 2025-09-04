// File: UserManagement.API/Endpoints/Incomes/List.cs
using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Incomes;

// --- Request DTO (No change here) ---
public class ListIncomesRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }

    [QueryParam]
    public int PageNumber { get; set; } = 1;

    [QueryParam]
    public int PageSize { get; set; } = 10;

    [QueryParam]
    public string? SearchTerm { get; set; }

    [QueryParam]
    public IncomeStatus? Status { get; set; }

    [QueryParam]
    public Guid? IncomeTypeId { get; set; }

    [QueryParam]
    public Guid? CurrencyId { get; set; }
}

public class IncomeListItem {
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public Guid CurrencyId { get; set; }
    public string CurrencyName { get; set; } = default!;
    public string CurrencyCode { get; set; } = default!; // New: Currency Code
    public DateTime Date { get; set; }
    public IncomeStatus IncomeStatus { get; set; }
    public string? Description { get; set; }
    public Guid IncomeTypeId { get; set; }
    public string IncomeTypeName { get; set; } = default!;
    public DateTime CreatedOn { get; set; }
    public Guid CreatedById { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public Guid? LastModifiedById { get; set; }
}

public class ListIncomesResponse {
    public IEnumerable<IncomeListItem> Incomes { get; set; } = Enumerable.Empty<IncomeListItem>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ListIncomesRequestValidator : Validator<ListIncomesRequest> {
    public ListIncomesRequestValidator() {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}

public class ListIncomes(AppDbContext dbContext) : Endpoint<ListIncomesRequest, ListIncomesResponse> {
    public override void Configure() {
        Get("incomes");
        Version(1);
        Permissions(nameof(UserPermission.Incomes_Read));
        Summary(s => {
            s.Summary = "Lists incomes with pagination, search, and filters";
            s.Description = "Retrieves a paginated list of incomes, optionally filtered by search term, status, income type, or currency.";
            s.ExampleRequest = new ListIncomesRequest {
                PageNumber = 1,
                PageSize = 5,
                SearchTerm = "bonus",
                Status = IncomeStatus.Received,
                IncomeTypeId = Guid.NewGuid(),
                CurrencyId = Guid.NewGuid()
            };
            s.ResponseExamples[200] = new ListIncomesResponse {
                Incomes = new[]
                {
                    new IncomeListItem
                    {
                        Id = Guid.NewGuid(),
                        Amount = 1500.00m,
                        CurrencyId = Guid.NewGuid(),
                        CurrencyName = "US Dollar",
                        CurrencyCode = "USD", // Updated example
                        Date = DateTime.UtcNow.AddDays(-5),
                        IncomeStatus = IncomeStatus.Received,
                        Description = "Monthly salary payment",
                        IncomeTypeId = Guid.NewGuid(),
                        IncomeTypeName = "Salary",
                        CreatedOn = DateTime.UtcNow.AddDays(-10),
                        LastModifiedOn = DateTime.UtcNow.AddDays(-2),
                        CreatedById = Guid.NewGuid(),
                        LastModifiedById = Guid.NewGuid()
                    }
                },
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 5,
                TotalPages = 1
            };
        });
    }

    public override async Task HandleAsync(ListIncomesRequest req, CancellationToken ct) {
        var query = dbContext.Incomes
            .Include(i => i.IncomeType)
            .Include(i => i.Currency)
            .AsQueryable();

        // HasQueryFilter in AppDbContext already filters out DeletedOn != null

        if (!string.IsNullOrWhiteSpace(req.SearchTerm)) {
            var searchTermLower = req.SearchTerm.ToLower();
            query = query.Where(i =>
                (i.Description != null && i.Description.ToLower().Contains(searchTermLower)) ||
                i.IncomeType.Name.ToLower().Contains(searchTermLower) ||
                i.Currency.Name.ToLower().Contains(searchTermLower) || // Search by Currency Name
                i.Currency.Code.ToLower().Contains(searchTermLower)); // New: Search by Currency Code
        }

        if (req.Status.HasValue) {
            query = query.Where(i => i.IncomeStatus == req.Status.Value);
        }

        if (req.IncomeTypeId.HasValue) {
            query = query.Where(i => i.IncomeTypeId == req.IncomeTypeId.Value);
        }

        if (req.CurrencyId.HasValue) {
            query = query.Where(i => i.CurrencyId == req.CurrencyId.Value);
        }

        var totalCount = await query.CountAsync(ct);

        query = query.OrderByDescending(i => i.Date).ThenBy(i => i.CreatedOn);

        var incomes = await query
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(i => new IncomeListItem {
                Id = i.Id,
                Amount = i.Amount,
                CurrencyId = i.CurrencyId,
                CurrencyName = i.Currency.Name,
                CurrencyCode = i.Currency.Code, // Select Currency Code
                Date = i.Date,
                IncomeStatus = i.IncomeStatus,
                Description = i.Description,
                IncomeTypeId = i.IncomeTypeId,
                IncomeTypeName = i.IncomeType.Name,
                CreatedOn = i.CreatedOn,
                CreatedById = i.CreatedById,
                LastModifiedOn = i.LastModifiedOn,
                LastModifiedById = i.LastModifiedById
            })
            .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling((double)totalCount / req.PageSize);

        await SendOkAsync(new ListIncomesResponse {
            Incomes = incomes,
            TotalCount = totalCount,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
            TotalPages = totalPages
        }, ct);
    }
}