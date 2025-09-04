using FastEndpoints; // For [QueryParam]
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using System;
using UserManagement.API.Enums;

namespace UserManagement.API.Endpoints.Dashboards.Finances;

// Request DTO for Net Profit
public class NetProfitRequest {
    [QueryParam]
    public Guid CurrencyId { get; set; }

    [QueryParam]
    public DateTime? StartDate { get; set; }

    [QueryParam]
    public DateTime? EndDate { get; set; }
}

// Response DTO for Net Profit
public class NetProfitResponse {
    public Guid CurrencyId { get; set; }
    public decimal NetProfitAmount { get; set; }
    public string? CurrencyCode { get; set; }
    public string? CurrencyName { get; set; }
}

// Validator for Net Profit Request (identical logic to previous ones)
public class NetProfitRequestValidator : Validator<NetProfitRequest> {
    public NetProfitRequestValidator() {
        RuleFor(x => x.CurrencyId)
            .NotEmpty().WithMessage("Currency ID is required.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("End date must be greater than or equal to start date.");
    }
}

public class NetProfit(AppDbContext dbContext) : Endpoint<NetProfitRequest, NetProfitResponse> {
    public override void Configure() {
        Get("dashboard/finances/net-profit"); // Distinct route for net profit
        Version(1);
        Permissions(UserPermission.Finances_Overview.ToString()); // Same permission, or create a specific one
        Summary(s => {
            s.Summary = "Gets the net profit (received income - total expenses) for a specific currency.";
            s.Description = "Calculates the net profit by subtracting total expenses from total received income, filtered by currency and an optional date range.";
            s.RequestParam(x => x.CurrencyId, "The ID of the currency to calculate net profit for.");
            s.RequestParam(x => x.StartDate, "Optional: The start date for filtering income and expenses (inclusive).");
            s.RequestParam(x => x.EndDate, "Optional: The end date for filtering income and expenses (inclusive).");
            s.Response<NetProfitResponse>(200, "Successfully retrieved net profit.");
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

        // 3. Calculate Net Profit
        var netProfit = totalReceivedIncome - totalExpenses;

        var response = new NetProfitResponse {
            CurrencyId = req.CurrencyId,
            NetProfitAmount = netProfit,
            CurrencyCode = currency.Code,
            CurrencyName = currency.Name
        };

        await SendOkAsync(response, ct);
    }
}