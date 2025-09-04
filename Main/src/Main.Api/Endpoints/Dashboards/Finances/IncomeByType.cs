using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using UserManagement.API.Enums; // For IncomeStatus (if you choose to filter by received/pending)
using UserManagement.API.Models; // For Income, Currency, IncomeType models

namespace UserManagement.API.Endpoints.Dashboards.Finances;

public class IncomeByTypeResponse {
    public Guid CurrencyId { get; set; }
    public string? CurrencyCode { get; set; }
    public string? CurrencyName { get; set; }
    public List<IncomeTypeSummary> IncomeTypeSummaries { get; set; } = new();

    public class IncomeTypeSummary {
        public Guid IncomeTypeId { get; set; }
        public string? IncomeTypeName { get; set; }
        public decimal TotalAmount { get; set; }
    }
}

public class IncomeByTypeEndpoint(AppDbContext dbContext) : Endpoint<TotalIncomeRequest, IncomeByTypeResponse> {
    public override void Configure() {
        Get("dashboard/finances/income-by-type"); // New distinct route
        Version(1);
        Permissions(UserPermission.Finances_Overview.ToString()); // Adjust permission if needed
        Summary(s => {
            s.Summary = "Gets total income grouped by income type for a specific currency.";
            s.Description = "Calculates the sum of received income amounts for each income type, filtered by currency and an optional date range.";
            s.RequestParam(x => x.CurrencyId, "The ID of the currency to filter by.");
            s.RequestParam(x => x.StartDate, "Optional: The start date for filtering income (inclusive).");
            s.RequestParam(x => x.EndDate, "Optional: The end date for filtering income (inclusive).");
            s.Response<IncomeByTypeResponse>(200, "Successfully retrieved income by type.");
            s.Response(400, "Invalid request parameters.");
            s.Response(404, "Currency not found."); // Changed to 404 for currency, 200 with empty list for no income
        });
    }

    public override async Task HandleAsync(TotalIncomeRequest req, CancellationToken ct) {
        var currency = await dbContext.Currencies.FirstOrDefaultAsync(c => c.Id == req.CurrencyId, ct);
        if (currency == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        // Start with all received incomes for the specified currency
        // You could add a parameter to filter by IncomeStatus if you want to see pending by type, etc.
        var query = dbContext.Incomes
            .Where(i => i.CurrencyId == req.CurrencyId && i.IncomeStatus == IncomeStatus.Received); // Assuming 'Income by Type' implies received income

        if (req.StartDate.HasValue) {
            var startDate = req.StartDate.Value.ToUniversalTime();
            query = query.Where(i => i.Date.Date >= startDate.Date);
        }

        if (req.EndDate.HasValue) {
            var endDate = req.EndDate.Value.ToUniversalTime();
            query = query.Where(i => i.Date.Date <= endDate.Date);
        }

        // Group by IncomeType and sum the amounts
        var incomeSummaries = await query
            .GroupBy(i => new { i.IncomeTypeId, i.IncomeType.Name }) // Group by ID and Name to get both
            .Select(g => new IncomeByTypeResponse.IncomeTypeSummary // Project into the summary DTO
            {
                IncomeTypeId = g.Key.IncomeTypeId,
                IncomeTypeName = g.Key.Name,
                TotalAmount = g.Sum(i => i.Amount)
            })
            .OrderByDescending(x => x.TotalAmount) // Optional: order by amount for easier reading
            .ToListAsync(ct); // Execute query asynchronously

        var response = new IncomeByTypeResponse {
            CurrencyId = req.CurrencyId,
            CurrencyCode = currency.Code,
            CurrencyName = currency.Name,
            IncomeTypeSummaries = incomeSummaries
        };

        await SendOkAsync(response, ct);
    }
}