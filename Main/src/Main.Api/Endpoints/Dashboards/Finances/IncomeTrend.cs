using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using UserManagement.API.Enums; // For IncomeStatus, TrendInterval

namespace UserManagement.API.Endpoints.Dashboards.Finances;

public enum TrendInterval {
    Daily = 0,
    Monthly = 1,
    Yearly = 2
    // You could add Weekly, Quarterly, etc. later if needed
}

public class IncomeTrendRequest {
    [QueryParam]
    public Guid CurrencyId { get; set; }

    [QueryParam]
    public DateTime StartDate { get; set; } // Required

    [QueryParam]
    public DateTime EndDate { get; set; } // Required

    [QueryParam]
    public TrendInterval Interval { get; set; } // Required enum

    [QueryParam]
    public IncomeStatus? IncomeStatusFilter { get; set; } = IncomeStatus.Received; // Optional: filter by Received, Pending, or All (if null)
}

// Response DTO for Income Trend
public class IncomeTrendResponse {
    public Guid CurrencyId { get; set; }
    public string? CurrencyCode { get; set; }
    public string? CurrencyName { get; set; }
    public TrendInterval Interval { get; set; }
    public List<IncomeTrendDataPoint> TrendData { get; set; } = new();

    public class IncomeTrendDataPoint {
        public DateTime PeriodStart { get; set; } // Start of the day/month/year
        public decimal TotalAmount { get; set; }
    }
}

// Validator for Income Trend Request
public class IncomeTrendRequestValidator : Validator<IncomeTrendRequest> {
    public IncomeTrendRequestValidator() {
        RuleFor(x => x.CurrencyId)
            .NotEmpty().WithMessage("Currency ID is required.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be greater than or equal to start date.");

        RuleFor(x => x.Interval)
            .IsInEnum().WithMessage("Invalid interval specified.");
    }
}

public class IncomeTrend(AppDbContext dbContext) : Endpoint<IncomeTrendRequest, IncomeTrendResponse> {
    public override void Configure() {
        Get("dashboard/finances/income-trend"); // New distinct route
        Version(1);
        Permissions(UserPermission.Finances_Overview.ToString()); // Adjust permission if needed
        Summary(s => {
            s.Summary = "Gets income trend data over a specified period and interval.";
            s.Description = "Aggregates income amounts by daily, monthly, or yearly intervals for a given currency and date range. Optional income status filter.";
            s.RequestParam(x => x.CurrencyId, "The ID of the currency to filter by.");
            s.RequestParam(x => x.StartDate, "The start date (inclusive) for the trend period.");
            s.RequestParam(x => x.EndDate, "The end date (inclusive) for the trend period.");
            s.RequestParam(x => x.Interval, "The aggregation interval (Daily, Monthly, Yearly).");
            s.RequestParam(x => x.IncomeStatusFilter, "Optional: Filter by income status (Received, Pending). If omitted, includes all statuses.");
            s.Response<IncomeTrendResponse>(200, "Successfully retrieved income trend data.");
            s.Response(400, "Invalid request parameters.");
            s.Response(404, "Currency not found.");
        });
    }

    public override async Task HandleAsync(IncomeTrendRequest req, CancellationToken ct) {
        var currency = await dbContext.Currencies.FirstOrDefaultAsync(c => c.Id == req.CurrencyId, ct);
        if (currency == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        DateTime startDateUtc = req.StartDate.ToUniversalTime();
        DateTime endDateUtc = req.EndDate.ToUniversalTime();

        endDateUtc = endDateUtc.Date.AddDays(1).ToUniversalTime(); // Ensure it's end of day + 1, and UTC

        var query = dbContext.Incomes
                .Where(e => e.CurrencyId == req.CurrencyId &&
                            e.Date >= startDateUtc &&
                            e.Date < endDateUtc);

        // Apply optional IncomeStatus filter
        if (req.IncomeStatusFilter.HasValue) {
            query = query.Where(i => i.IncomeStatus == req.IncomeStatusFilter.Value);
        }

        var trendData = new List<IncomeTrendResponse.IncomeTrendDataPoint>();

        switch (req.Interval) {
            case TrendInterval.Daily:
                trendData = await query
                    .GroupBy(i => i.Date.Date) // Group by date only
                    .Select(g => new IncomeTrendResponse.IncomeTrendDataPoint {
                        PeriodStart = g.Key,
                        TotalAmount = g.Sum(i => i.Amount)
                    })
                    .OrderBy(x => x.PeriodStart)
                    .ToListAsync(ct);
                break;

            case TrendInterval.Monthly:
                trendData = await query
                    .GroupBy(i => new { i.Date.Year, i.Date.Month }) // Group by year and month
                    .Select(g => new IncomeTrendResponse.IncomeTrendDataPoint {
                        PeriodStart = new DateTime(g.Key.Year, g.Key.Month, 1), // Start of the month
                        TotalAmount = g.Sum(i => i.Amount)
                    })
                    .OrderBy(x => x.PeriodStart)
                    .ToListAsync(ct);
                break;

            case TrendInterval.Yearly:
                trendData = await query
                    .GroupBy(i => i.Date.Year) // Group by year
                    .Select(g => new IncomeTrendResponse.IncomeTrendDataPoint {
                        PeriodStart = new DateTime(g.Key, 1, 1), // Start of the year
                        TotalAmount = g.Sum(i => i.Amount)
                    })
                    .OrderBy(x => x.PeriodStart)
                    .ToListAsync(ct);
                break;
            default:
                // This case should ideally be caught by validation, but as a fallback:
                await SendErrorsAsync(400, ct);
                return;
        }

        var response = new IncomeTrendResponse {
            CurrencyId = req.CurrencyId,
            CurrencyCode = currency.Code,
            CurrencyName = currency.Name,
            Interval = req.Interval,
            TrendData = trendData
        };

        await SendOkAsync(response, ct);
    }
}