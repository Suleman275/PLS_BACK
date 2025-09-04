using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using UserManagement.API.Enums;

namespace UserManagement.API.Endpoints.Dashboards.Finances;

public class IncomeExpenseRatioResponse {
    public Guid CurrencyId { get; set; }
    public decimal? Ratio { get; set; } // Nullable decimal to handle division by zero scenarios
    public string? CurrencyCode { get; set; }
    public string? CurrencyName { get; set; }
    public string? Message { get; set; } // Optional: For messages like "No expenses for the period"
}

// Reuse NetProfitRequest (which aligns with the needed query parameters)
public class IncomeExpenseRatio(AppDbContext dbContext) : Endpoint<NetProfitRequest, IncomeExpenseRatioResponse> {
    public override void Configure() {
        Get("dashboard/finances/income-expense-ratio"); // Distinct route
        Version(1);
        Permissions(UserPermission.Finances_Overview.ToString()); // Adjust permission if needed
        Summary(s => {
            s.Summary = "Gets the income to expense ratio for a specific currency.";
            s.Description = "Calculates the ratio of total received income to total expenses, filtered by currency and an optional date range. Returns null ratio if total expenses are zero.";
            s.RequestParam(x => x.CurrencyId, "The ID of the currency to calculate the ratio for.");
            s.RequestParam(x => x.StartDate, "Optional: The start date for filtering income and expenses (inclusive).");
            s.RequestParam(x => x.EndDate, "Optional: The end date for filtering income and expenses (inclusive).");
            s.Response<IncomeExpenseRatioResponse>(200, "Successfully retrieved income to expense ratio.");
            s.Response(400, "Invalid request parameters.");
            s.Response(404, "Currency not found.");
        });
    }

    public override async Task HandleAsync(NetProfitRequest req, CancellationToken ct) {
        var currency = await dbContext.Currencies.FirstOrDefaultAsync(c => c.Id == req.CurrencyId, ct);
        if (currency == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        // 1. Calculate Total Received Income
        var incomeQuery = dbContext.Incomes
            .Where(i => i.CurrencyId == req.CurrencyId && i.IncomeStatus == IncomeStatus.Received);

        if (req.StartDate.HasValue) {
            var startDate = req.StartDate.Value.ToUniversalTime();
            incomeQuery = incomeQuery.Where(i => i.Date.Date >= startDate.Date);
        }

        if (req.EndDate.HasValue) {
            var endDate = req.EndDate.Value.ToUniversalTime();
            incomeQuery = incomeQuery.Where(i => i.Date.Date <= endDate.Date);
        }

        var totalReceivedIncome = await incomeQuery.SumAsync(i => i.Amount, ct);

        // 2. Calculate Total Expenses
        var expenseQuery = dbContext.Expenses
            .Where(e => e.CurrencyId == req.CurrencyId);

        if (req.StartDate.HasValue) {
            var startDate = req.StartDate.Value.ToUniversalTime();
            expenseQuery = expenseQuery.Where(e => e.Date.Date >= startDate.Date);
        }

        if (req.EndDate.HasValue) {
            var endDate = req.EndDate.Value.ToUniversalTime();
            expenseQuery = expenseQuery.Where(e => e.Date.Date <= endDate.Date);
        }

        var totalExpenses = await expenseQuery.SumAsync(e => e.Amount, ct);

        // 3. Calculate Ratio and Handle Division by Zero
        decimal? ratio = null;
        string? message = null;

        if (totalExpenses == 0) {
            if (totalReceivedIncome > 0) {
                message = "Cannot calculate ratio: No expenses recorded (infinite ratio).";
                // You could technically return decimal.MaxValue or a specific indicator,
                // but null with a message is often clearer for API consumers.
            }
            else {
                message = "No income and no expenses recorded for the period.";
            }
        }
        else {
            ratio = totalReceivedIncome / totalExpenses;
        }

        var response = new IncomeExpenseRatioResponse {
            CurrencyId = req.CurrencyId,
            Ratio = ratio,
            CurrencyCode = currency.Code,
            CurrencyName = currency.Name,
            Message = message
        };

        await SendOkAsync(response, ct);
    }
}