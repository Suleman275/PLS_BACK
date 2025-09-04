using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using UserManagement.API.Enums;

namespace UserManagement.API.Endpoints.Dashboards.Finances;

public class TotalIncomeRequest {
    [QueryParam]
    public Guid CurrencyId { get; set; }

    [QueryParam]
    public DateTime? StartDate { get; set; }

    [QueryParam]
    public DateTime? EndDate { get; set; }
}

public class TotalIncomeResponse {
    public Guid CurrencyId { get; set; }
    public decimal TotalAmount { get; set; }
    public string? CurrencyCode { get; set; }
    public string? CurrencyName { get; set; }
}

public class TotalIncomeRequestValidator : Validator<TotalIncomeRequest> {
    public TotalIncomeRequestValidator() {
        RuleFor(x => x.CurrencyId)
            .NotEmpty().WithMessage("Currency ID is required.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("End date must be greater than or equal to start date.");
    }
}

public class TotalIncome(AppDbContext dbContext) : Endpoint<TotalIncomeRequest, TotalIncomeResponse> {
    public override void Configure() {
        Get("dashboard/finances/total-income");
        Version(1);
        Permissions(nameof(UserPermission.Finances_Overview));
        Summary(s => {
            s.Summary = "Gets the total received income for a specific currency.";
            s.Description = "Calculates the sum of all received income amounts for a given currency, with optional date range filtering.";
            s.RequestParam(x => x.CurrencyId, "The ID of the currency to filter by.");
            s.RequestParam(x => x.StartDate, "Optional: The start date for filtering income (inclusive).");
            s.RequestParam(x => x.EndDate, "Optional: The end date for filtering income (inclusive).");
            s.Response<TotalIncomeResponse>(200, "Successfully retrieved total income by currency.");
            s.Response(400, "Invalid request parameters.");
            s.Response(404, "Currency not found or no income records for the specified currency and date range.");
        });
    }

    public override async Task HandleAsync(TotalIncomeRequest req, CancellationToken ct) {
        var currency = await dbContext.Currencies.FirstOrDefaultAsync(c => c.Id == req.CurrencyId, ct);
        if (currency == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        var query = dbContext.Incomes.Where(i => i.CurrencyId == req.CurrencyId && i.IncomeStatus == IncomeStatus.Received); // Only received income for "Total Income"

        if (req.StartDate.HasValue) {
            var startDate = req.StartDate.Value.ToUniversalTime();
            query = query.Where(i => i.Date.Date >= startDate.Date);
        }

        if (req.EndDate.HasValue) {
            var endDate = req.EndDate.Value.ToUniversalTime();
            query = query.Where(i => i.Date.Date <= endDate.Date);
        }

        var totalAmount = await query.SumAsync(i => i.Amount, ct);

        var response = new TotalIncomeResponse {
            CurrencyId = req.CurrencyId,
            TotalAmount = totalAmount,
            CurrencyCode = currency.Code,
            CurrencyName = currency.Name
        };

        await SendOkAsync(response, ct);
    }
}