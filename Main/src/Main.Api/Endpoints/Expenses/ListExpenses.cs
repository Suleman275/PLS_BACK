// File: UserManagement.API/Endpoints/Expenses/List.cs
using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Expenses;

// --- Request DTO (No change here) ---
public class ListExpensesRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }

    [QueryParam]
    public int PageNumber { get; set; } = 1;

    [QueryParam]
    public int PageSize { get; set; } = 10;

    [QueryParam]
    public string? SearchTerm { get; set; }

    [QueryParam]
    public Guid? ExpenseTypeId { get; set; }

    [QueryParam]
    public Guid? CurrencyId { get; set; }
}

// --- Item DTO for the list response ---
public class ExpenseListItem {
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public Guid CurrencyId { get; set; }
    public string CurrencyName { get; set; } = default!;
    public string CurrencyCode { get; set; } = default!; // New: Currency Code
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public Guid ExpenseTypeId { get; set; }
    public string ExpenseTypeName { get; set; } = default!;
    public DateTime CreatedOn { get; set; }
    public Guid CreatedById { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public Guid? LastModifiedById { get; set; }
}

// --- Response DTO (No change here) ---
public class ListExpensesResponse {
    public IEnumerable<ExpenseListItem> Expenses { get; set; } = Enumerable.Empty<ExpenseListItem>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

// --- Request Validator (No change here) ---
public class ListExpensesRequestValidator : Validator<ListExpensesRequest> {
    public ListExpensesRequestValidator() {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}

public class ListExpenses(AppDbContext dbContext) : Endpoint<ListExpensesRequest, ListExpensesResponse> {
    public override void Configure() {
        Get("expenses");
        Version(1);
        Permissions(nameof(UserPermission.Expenses_Read));
        Summary(s => {
            s.Summary = "Lists expenses with pagination, search, and filters";
            s.Description = "Retrieves a paginated list of expenses, optionally filtered by search term, expense type, or currency.";
            s.ExampleRequest = new ListExpensesRequest {
                PageNumber = 1,
                PageSize = 5,
                SearchTerm = "groceries",
                ExpenseTypeId = Guid.NewGuid(),
                CurrencyId = Guid.NewGuid()
            };
            s.ResponseExamples[200] = new ListExpensesResponse {
                Expenses = new[]
                {
                    new ExpenseListItem
                    {
                        Id = Guid.NewGuid(),
                        Amount = 120.50m,
                        CurrencyId = Guid.NewGuid(),
                        CurrencyName = "Euro",
                        CurrencyCode = "EUR", // Updated example
                        Date = DateTime.UtcNow.AddDays(-3),
                        Description = "Weekly groceries shopping",
                        ExpenseTypeId = Guid.NewGuid(),
                        ExpenseTypeName = "Food",
                        CreatedOn = DateTime.UtcNow.AddDays(-5),
                        LastModifiedOn = null,
                        CreatedById = Guid.NewGuid(),
                        LastModifiedById = null
                    },
                    new ExpenseListItem
                    {
                        Id = Guid.NewGuid(),
                        Amount = 45.00m,
                        CurrencyId = Guid.NewGuid(),
                        CurrencyName = "US Dollar",
                        CurrencyCode = "USD", // Updated example
                        Date = DateTime.UtcNow.AddDays(-1),
                        Description = "Coffee with client",
                        ExpenseTypeId = Guid.NewGuid(),
                        ExpenseTypeName = "Entertainment",
                        CreatedOn = DateTime.UtcNow.AddDays(-2),
                        LastModifiedOn = DateTime.UtcNow.AddHours(-6),
                        CreatedById = Guid.NewGuid(),
                        LastModifiedById = Guid.NewGuid()
                    }
                },
                TotalCount = 2,
                PageNumber = 1,
                PageSize = 5,
                TotalPages = 1
            };
        });
    }

    public override async Task HandleAsync(ListExpensesRequest req, CancellationToken ct) {
        var query = dbContext.Expenses
            .Include(e => e.ExpenseType)
            .Include(e => e.Currency)
            .AsQueryable();

        // HasQueryFilter in AppDbContext already filters out DeletedOn != null

        if (!string.IsNullOrWhiteSpace(req.SearchTerm)) {
            var searchTermLower = req.SearchTerm.ToLower();
            query = query.Where(e =>
                (e.Description != null && e.Description.ToLower().Contains(searchTermLower)) ||
                e.ExpenseType.Name.ToLower().Contains(searchTermLower) ||
                e.Currency.Name.ToLower().Contains(searchTermLower) || // Search by Currency Name
                e.Currency.Code.ToLower().Contains(searchTermLower)); // New: Search by Currency Code
        }

        if (req.ExpenseTypeId.HasValue) {
            query = query.Where(e => e.ExpenseTypeId == req.ExpenseTypeId.Value);
        }

        if (req.CurrencyId.HasValue) {
            query = query.Where(e => e.CurrencyId == req.CurrencyId.Value);
        }

        var totalCount = await query.CountAsync(ct);

        query = query.OrderByDescending(e => e.Date).ThenBy(e => e.CreatedOn);

        var expenses = await query
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(e => new ExpenseListItem {
                Id = e.Id,
                Amount = e.Amount,
                CurrencyId = e.CurrencyId,
                CurrencyName = e.Currency.Name,
                CurrencyCode = e.Currency.Code, // Select Currency Code
                Date = e.Date,
                Description = e.Description,
                ExpenseTypeId = e.ExpenseTypeId,
                ExpenseTypeName = e.ExpenseType.Name,
                CreatedOn = e.CreatedOn,
                CreatedById = e.CreatedById,
                LastModifiedOn = e.LastModifiedOn,
                LastModifiedById = e.LastModifiedById
            })
            .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling((double)totalCount / req.PageSize);

        await SendOkAsync(new ListExpensesResponse {
            Expenses = expenses,
            TotalCount = totalCount,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
            TotalPages = totalPages
        }, ct);
    }
}