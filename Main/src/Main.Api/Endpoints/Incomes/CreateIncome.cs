// File: UserManagement.API/Endpoints/Incomes/Create.cs
using FastEndpoints;
using FluentValidation;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Enums;
using UserManagement.API.Models;
using Microsoft.EntityFrameworkCore;
using Main.Api.Data;

namespace UserManagement.API.Endpoints.Incomes;

// --- Request DTO (No change here) ---
public class CreateIncomeRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    public decimal Amount { get; set; }
    public Guid CurrencyId { get; set; }
    public DateTime Date { get; set; }
    public IncomeStatus IncomeStatus { get; set; }
    public string? Description { get; set; }
    public Guid IncomeTypeId { get; set; }
}

// --- Response DTO ---
public class CreateIncomeResponse {
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public Guid CurrencyId { get; set; }
    public string CurrencyName { get; set; } = default!;
    public string CurrencyCode { get; set; } = default!; // New: Currency Code
    public DateTime Date { get; set; }
    public IncomeStatus IncomeStatus { get; set; }
    public string? Description { get; set; }
    public Guid IncomeTypeId { get; set; }
    public string IncomeTypeName { get; set; } = default!;
    public DateTime CreatedOn { get; set; }
}

// --- Request Validator (No change here) ---
public class CreateIncomeRequestValidator : Validator<CreateIncomeRequest> {
    public CreateIncomeRequestValidator() {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0.");

        RuleFor(x => x.CurrencyId)
            .NotEmpty().WithMessage("Currency ID is required.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required.")
            .LessThanOrEqualTo(DateTime.UtcNow.AddHours(1)).WithMessage("Date cannot be in the future.");

        RuleFor(x => x.IncomeStatus)
            .IsInEnum().WithMessage("Invalid IncomeStatus value.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.IncomeTypeId)
            .NotEmpty().WithMessage("Income type ID is required.");
    }
}

public class CreateIncome(AppDbContext dbContext) : Endpoint<CreateIncomeRequest, CreateIncomeResponse> {
    public override void Configure() {
        Post("incomes");
        Version(1);
        Permissions(nameof(UserPermission.Incomes_Create));
        Summary(s => {
            s.Summary = "Creates a new income record";
            s.Description = "Adds a new income entry to the system with details like amount, currency (by ID), date, status, description, and income type.";
            s.ExampleRequest = new CreateIncomeRequest {
                Amount = 1200.50m,
                CurrencyId = Guid.NewGuid(),
                Date = DateTime.UtcNow.Date,
                IncomeStatus = IncomeStatus.Pending,
                Description = "Client project payment for Q3",
                IncomeTypeId = Guid.NewGuid()
            };
            s.ResponseExamples[201] = new CreateIncomeResponse {
                Id = Guid.NewGuid(),
                Amount = 1200.50m,
                CurrencyId = Guid.NewGuid(),
                CurrencyName = "Euro",
                CurrencyCode = "EUR", // Updated example
                Date = DateTime.UtcNow.Date,
                IncomeStatus = IncomeStatus.Pending,
                Description = "Client project payment for Q3",
                IncomeTypeId = Guid.NewGuid(),
                IncomeTypeName = "Project Income",
                CreatedOn = DateTime.UtcNow
            };
        });
    }

    public override async Task HandleAsync(CreateIncomeRequest req, CancellationToken ct) {
        var incomeType = await dbContext.IncomeTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(it => it.Id == req.IncomeTypeId, ct);

        if (incomeType == null) {
            AddError(req => req.IncomeTypeId, "Specified Income Type does not exist.");
            await SendErrorsAsync(400, ct);
            return;
        }

        var currency = await dbContext.Currencies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == req.CurrencyId, ct);

        if (currency == null) {
            AddError(req => req.CurrencyId, "Specified Currency does not exist.");
            await SendErrorsAsync(400, ct);
            return;
        }

        var newIncome = new Income {
            Amount = req.Amount,
            CurrencyId = req.CurrencyId,
            Date = req.Date.Date,
            IncomeStatus = req.IncomeStatus,
            Description = req.Description,
            IncomeTypeId = req.IncomeTypeId,
            CreatedById = req.SubjectId,
        };

        dbContext.Incomes.Add(newIncome);

        await dbContext.SaveChangesAsync(ct);

        await SendAsync(new CreateIncomeResponse {
            Id = newIncome.Id,
            Amount = newIncome.Amount,
            CurrencyId = newIncome.CurrencyId,
            CurrencyName = currency.Name,
            CurrencyCode = currency.Code, // Populate Currency Code
            Date = newIncome.Date,
            IncomeStatus = newIncome.IncomeStatus,
            Description = newIncome.Description,
            IncomeTypeId = newIncome.IncomeTypeId,
            IncomeTypeName = incomeType.Name,
            CreatedOn = newIncome.CreatedOn
        }, 201, ct);
    }
}