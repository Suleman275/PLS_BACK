using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.Dashboards.Finances;

public class ExpensesByType(AppDbContext dbContext) : Endpoint<ExpenseRequest, ExpensesByTypeResponse> {
    public override void Configure() {
        Get("dashboard/finances/expenses-by-type"); // New distinct route
        Version(1);
        Permissions(UserPermission.Finances_Overview.ToString()); // Or specific expense permission
        Summary(s => {
            s.Summary = "Gets total expenses grouped by expense type for a specific currency.";
            s.Description = "Calculates the sum of expense amounts for each expense type, filtered by currency and an optional date range.";
            s.RequestParam(x => x.CurrencyId, "The ID of the currency to filter by.");
            s.RequestParam(x => x.StartDate, "Optional: The start date for filtering expenses (inclusive).");
            s.RequestParam(x => x.EndDate, "Optional: The end date for filtering expenses (inclusive).");
            s.Response<ExpensesByTypeResponse>(200, "Successfully retrieved expenses by type.");
            s.Response(400, "Invalid request parameters.");
            s.Response(404, "Currency not found.");
        });
    }

    public override async Task HandleAsync(ExpenseRequest req, CancellationToken ct) {
        var currency = await dbContext.Currencies.FirstOrDefaultAsync(c => c.Id == req.CurrencyId, ct);
        if (currency == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        var query = dbContext.Expenses
            .Where(e => e.CurrencyId == req.CurrencyId);

        if (req.StartDate.HasValue) {
            var startDate = req.StartDate.Value.ToUniversalTime();
            query = query.Where(e => e.Date.Date >= startDate.Date);
        }

        if (req.EndDate.HasValue) {
            var endDate = req.EndDate.Value.ToUniversalTime();
            query = query.Where(e => e.Date.Date <= endDate.Date);
        }

        // Group by ExpenseType and sum the amounts
        var expenseSummaries = await query
            .GroupBy(e => new { e.ExpenseTypeId, e.ExpenseType.Name }) // Group by ID and Name
            .Select(g => new ExpensesByTypeResponse.ExpenseTypeSummary // Project into the summary DTO
            {
                ExpenseTypeId = g.Key.ExpenseTypeId,
                ExpenseTypeName = g.Key.Name,
                TotalAmount = g.Sum(e => e.Amount)
            })
            .OrderByDescending(x => x.TotalAmount) // Optional: order by amount
            .ToListAsync(ct); // Execute query asynchronously

        var response = new ExpensesByTypeResponse {
            CurrencyId = req.CurrencyId,
            CurrencyCode = currency.Code,
            CurrencyName = currency.Name,
            ExpenseTypeSummaries = expenseSummaries
        };

        await SendOkAsync(response, ct);
    }
}