using FastEndpoints; // For [QueryParam]
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.Dashboards.Finances;

// Reuse request structure for total expenses and expenses by type
public class ExpenseRequest {
    [QueryParam]
    public Guid CurrencyId { get; set; }

    [QueryParam]
    public DateTime? StartDate { get; set; }

    [QueryParam]
    public DateTime? EndDate { get; set; }
}

// Validator for expense requests (same logic as income)
public class ExpenseRequestValidator : Validator<ExpenseRequest> {
    public ExpenseRequestValidator() {
        RuleFor(x => x.CurrencyId)
            .NotEmpty().WithMessage("Currency ID is required.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("End date must be greater than or equal to start date.");
    }
}

// Response for Total Expenses
public class TotalExpensesResponse {
    public Guid CurrencyId { get; set; }
    public decimal TotalAmount { get; set; }
    public string? CurrencyCode { get; set; }
    public string? CurrencyName { get; set; }
}

// Response for Expenses by Type
public class ExpensesByTypeResponse {
    public Guid CurrencyId { get; set; }
    public string? CurrencyCode { get; set; }
    public string? CurrencyName { get; set; }
    public List<ExpenseTypeSummary> ExpenseTypeSummaries { get; set; } = new();

    public class ExpenseTypeSummary {
        public Guid ExpenseTypeId { get; set; }
        public string? ExpenseTypeName { get; set; }
        public decimal TotalAmount { get; set; }
    }
}

public class TotalExpense(AppDbContext dbContext) : Endpoint<ExpenseRequest, TotalExpensesResponse> {
    public override void Configure() {
        Get("dashboard/finances/total-expenses");
        Version(1);
        Permissions(nameof(UserPermission.Finances_Overview));
        Summary(s => {
            s.Summary = "Gets the total expenses for a specific currency.";
            s.Description = "Calculates the sum of all expense amounts for a given currency, with optional date range filtering.";
            s.RequestParam(x => x.CurrencyId, "The ID of the currency to filter by.");
            s.RequestParam(x => x.StartDate, "Optional: The start date for filtering expenses (inclusive).");
            s.RequestParam(x => x.EndDate, "Optional: The end date for filtering expenses (inclusive).");
            s.Response<TotalExpensesResponse>(200, "Successfully retrieved total expenses by currency.");
            s.Response(400, "Invalid request parameters.");
            s.Response(404, "Currency not found or no expense records for the specified currency and date range.");
        });
    }

    public override async Task HandleAsync(ExpenseRequest req, CancellationToken ct) {
        var currency = await dbContext.Currencies.FirstOrDefaultAsync(c => c.Id == req.CurrencyId, ct);
        if (currency == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        var query = dbContext.Expenses.Where(e => e.CurrencyId == req.CurrencyId);

        if (req.StartDate.HasValue) {
            var startDate = req.StartDate.Value.ToUniversalTime();
            query = query.Where(e => e.Date.Date >= startDate.Date);
        }

        if (req.EndDate.HasValue) {
            var endDate = req.EndDate.Value.ToUniversalTime();
            query = query.Where(e => e.Date.Date <= endDate.Date);
        }

        var totalAmount = await query.SumAsync(e => e.Amount, ct);

        var response = new TotalExpensesResponse {
            CurrencyId = req.CurrencyId,
            TotalAmount = totalAmount,
            CurrencyCode = currency.Code,
            CurrencyName = currency.Name
        };

        await SendOkAsync(response, ct);
    }
}