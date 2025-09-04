using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
// Removed UserManagement.API.Enums because we are not filtering by IncomeStatus here
using UserManagement.API.Models; // For Currency model

namespace UserManagement.API.Endpoints.Dashboards.Finances;

// Reuse NetProfitRequest and NetProfitResponse from NetProfitDtos.cs
public class NetExpectedProfit(AppDbContext dbContext) : Endpoint<NetProfitRequest, NetProfitResponse> {
    public override void Configure() {
        Get("dashboard/finances/net-expected-profit"); // Distinct route for net expected profit
        Version(1);
        Permissions(UserPermission.Finances_Overview.ToString()); // Consider if this needs a separate permission
        Summary(s => {
            s.Summary = "Gets the net expected profit (all income - total expenses) for a specific currency.";
            s.Description = "Calculates the net expected profit by subtracting total expenses from ALL income (received and pending), filtered by currency and an optional date range.";
            s.RequestParam(x => x.CurrencyId, "The ID of the currency to calculate net expected profit for.");
            s.RequestParam(x => x.StartDate, "Optional: The start date for filtering income and expenses (inclusive).");
            s.RequestParam(x => x.EndDate, "Optional: The end date for filtering income and expenses (inclusive).");
            s.Response<NetProfitResponse>(200, "Successfully retrieved net expected profit.");
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

        // 1. Calculate Total Income (regardless of status - Received or Pending)
        var incomeQuery = dbContext.Incomes
            .Where(i => i.CurrencyId == req.CurrencyId); // NO IncomeStatus filter here

        if (req.StartDate.HasValue) {
            var startDate = req.StartDate.Value.ToUniversalTime();
            incomeQuery = incomeQuery.Where(i => i.Date.Date >= startDate.Date);
        }

        if (req.EndDate.HasValue) {
            var endDate = req.EndDate.Value.ToUniversalTime();
            incomeQuery = incomeQuery.Where(i => i.Date.Date <= endDate.Date);
        }

        var totalAllIncome = await incomeQuery.SumAsync(i => i.Amount, ct);

        // 2. Calculate Total Expenses (this part remains the same)
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

        // 3. Calculate Net Expected Profit
        var netExpectedProfit = totalAllIncome - totalExpenses;

        var response = new NetProfitResponse // Using NetProfitResponse DTO, perhaps rename if desired for clarity
        {
            CurrencyId = req.CurrencyId,
            NetProfitAmount = netExpectedProfit, // Renaming this property might be clearer if it's "NetExpectedProfitAmount"
            CurrencyCode = currency.Code,
            CurrencyName = currency.Name
        };

        await SendOkAsync(response, ct);
    }
}