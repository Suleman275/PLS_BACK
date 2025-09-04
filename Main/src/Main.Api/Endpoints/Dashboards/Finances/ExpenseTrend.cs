using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using UserManagement.API.Enums; // For TrendInterval
using UserManagement.API.Models; // For Expense, Currency models
using System.Linq;
using FluentValidation;
using Main.Api.Data; // For LINQ extension methods

namespace UserManagement.API.Endpoints.Dashboards.Finances;

// Request DTO for Expense Trend (similar to IncomeTrendRequest, but for clarity)
public class ExpenseTrendRequest {
    [QueryParam]
    public Guid CurrencyId { get; set; }

    [QueryParam]
    public DateTime StartDate { get; set; } // Required

    [QueryParam]
    public DateTime EndDate { get; set; } // Required

    [QueryParam]
    public TrendInterval Interval { get; set; } // Required enum
}

// Response DTO for Expense Trend (similar to IncomeTrendResponse)
public class ExpenseTrendResponse {
    public Guid CurrencyId { get; set; }
    public string? CurrencyCode { get; set; }
    public string? CurrencyName { get; set; }
    public TrendInterval Interval { get; set; }
    public List<ExpenseTrendDataPoint> TrendData { get; set; } = new();

    public class ExpenseTrendDataPoint {
        public DateTime PeriodStart { get; set; } // Start of the day/month/year
        public decimal TotalAmount { get; set; }
    }
}

// Validator for Expense Trend Request (identical logic to IncomeTrendRequestValidator)
public class ExpenseTrendRequestValidator : Validator<ExpenseTrendRequest> {
    public ExpenseTrendRequestValidator() {
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

public class ExpenseTrend(AppDbContext dbContext) : Endpoint<ExpenseTrendRequest, ExpenseTrendResponse> {
    public override void Configure() {
        Get("dashboard/finances/expense-trend"); // New distinct route
        Version(1);
        Permissions(UserPermission.Finances_Overview.ToString()); // Adjust permission if needed
        Summary(s => {
            s.Summary = "Gets expense trend data over a specified period and interval.";
            s.Description = "Aggregates expense amounts by daily, monthly, or yearly intervals for a given currency and date range.";
            s.RequestParam(x => x.CurrencyId, "The ID of the currency to filter by.");
            s.RequestParam(x => x.StartDate, "The start date (inclusive) for the trend period.");
            s.RequestParam(x => x.EndDate, "The end date (inclusive) for the trend period.");
            s.RequestParam(x => x.Interval, "The aggregation interval (Daily, Monthly, Yearly).");
            s.Response<ExpenseTrendResponse>(200, "Successfully retrieved expense trend data.");
            s.Response(400, "Invalid request parameters.");
            s.Response(404, "Currency not found.");
        });
    }

    public override async Task HandleAsync(ExpenseTrendRequest req, CancellationToken ct) {
        var currency = await dbContext.Currencies.FirstOrDefaultAsync(c => c.Id == req.CurrencyId, ct);
        if (currency == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        DateTime startDateUtc = req.StartDate.ToUniversalTime();
        DateTime endDateUtc = req.EndDate.ToUniversalTime();

        endDateUtc = endDateUtc.Date.AddDays(1).ToUniversalTime(); // Ensure it's end of day + 1, and UTC



        var query = dbContext.Expenses
                .Where(e => e.CurrencyId == req.CurrencyId &&
                            e.Date >= startDateUtc &&
                            e.Date < endDateUtc);

        var trendData = new List<ExpenseTrendResponse.ExpenseTrendDataPoint>();

        switch (req.Interval) {
            case TrendInterval.Daily:
                trendData = await query
                    .GroupBy(e => e.Date.Date) // Group by date only
                    .Select(g => new ExpenseTrendResponse.ExpenseTrendDataPoint {
                        PeriodStart = g.Key,
                        TotalAmount = g.Sum(e => e.Amount)
                    })
                    .OrderBy(x => x.PeriodStart)
                    .ToListAsync(ct);
                break;

            case TrendInterval.Monthly:
                trendData = await query
                    .GroupBy(e => new { e.Date.Year, e.Date.Month }) // Group by year and month
                    .Select(g => new ExpenseTrendResponse.ExpenseTrendDataPoint {
                        PeriodStart = new DateTime(g.Key.Year, g.Key.Month, 1), // Start of the month
                        TotalAmount = g.Sum(e => e.Amount)
                    })
                    .OrderBy(x => x.PeriodStart)
                    .ToListAsync(ct);
                break;

            case TrendInterval.Yearly:
                trendData = await query
                    .GroupBy(e => e.Date.Year) // Group by year
                    .Select(g => new ExpenseTrendResponse.ExpenseTrendDataPoint {
                        PeriodStart = new DateTime(g.Key, 1, 1), // Start of the year
                        TotalAmount = g.Sum(e => e.Amount)
                    })
                    .OrderBy(x => x.PeriodStart)
                    .ToListAsync(ct);
                break;
            default:
                await SendErrorsAsync(400, ct);
                return;
        }

        var response = new ExpenseTrendResponse {
            CurrencyId = req.CurrencyId,
            CurrencyCode = currency.Code,
            CurrencyName = currency.Name,
            Interval = req.Interval,
            TrendData = trendData
        };

        await SendOkAsync(response, ct);
    }
}