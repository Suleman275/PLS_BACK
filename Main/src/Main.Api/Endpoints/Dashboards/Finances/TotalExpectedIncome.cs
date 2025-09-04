using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore; // Required for async EF Core methods
using SharedKernel.Enums; // Assuming UserPermissions is in SharedKernel.Enums
using UserManagement.API.Enums; // Your IncomeStatus enum

namespace UserManagement.API.Endpoints.Dashboards.Finances;

// We are reusing TotalIncomeRequest and TotalIncomeResponse as their structures are identical
public class TotalExpectedIncome(AppDbContext dbContext) : Endpoint<TotalIncomeRequest, TotalIncomeResponse> {
    public override void Configure() {
        Get("dashboard/finances/total-expected-income"); // Distinct route
        Version(1);
        Permissions(UserPermission.Finances_Overview.ToString()); // Same permission, adjust if needed
        Summary(s => {
            s.Summary = "Gets the total expected (pending) income for a specific currency.";
            s.Description = "Calculates the sum of all pending income amounts for a given currency, with optional date range filtering.";
            s.RequestParam(x => x.CurrencyId, "The ID of the currency to filter by.");
            s.RequestParam(x => x.StartDate, "Optional: The start date for filtering expected income (inclusive).");
            s.RequestParam(x => x.EndDate, "Optional: The end date for filtering expected income (inclusive).");
            s.Response<TotalIncomeResponse>(200, "Successfully retrieved total expected income by currency.");
            s.Response(400, "Invalid request parameters.");
            s.Response(404, "Currency not found or no pending income records for the specified currency and date range.");
        });
    }

    public override async Task HandleAsync(TotalIncomeRequest req, CancellationToken ct) {
        var currency = await dbContext.Currencies.FirstOrDefaultAsync(c => c.Id == req.CurrencyId, ct);
        if (currency == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        // The key change: filter by IncomeStatus.Pending
        var query = dbContext.Incomes
            .Where(i => i.CurrencyId == req.CurrencyId);

        if (req.StartDate.HasValue) {
            var startDate = req.StartDate.Value.ToUniversalTime();
            query = query.Where(i => i.Date.Date >= startDate.Date);
        }

        if (req.EndDate.HasValue) {
            var endDate = req.EndDate.Value.ToUniversalTime();
            query = query.Where(i => i.Date.Date <= endDate.Date);
        }

        var totalAmount = await query.SumAsync(i => i.Amount, ct);

        // For expected income, sending 0 if no records match is also a valid response.
        var response = new TotalIncomeResponse {
            CurrencyId = req.CurrencyId,
            TotalAmount = totalAmount,
            CurrencyCode = currency.Code,
            CurrencyName = currency.Name
        };

        await SendOkAsync(response, ct);
    }
}