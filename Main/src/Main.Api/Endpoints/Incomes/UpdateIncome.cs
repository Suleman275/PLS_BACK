// File: UserManagement.API/Endpoints/Incomes/Update.cs
using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Incomes;

// --- Request DTO (No change here) ---
public class UpdateIncomeRequest {
    [FromRoute]
    public Guid Id { get; set; }
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
public class UpdateIncomeResponse {
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
    public DateTime? LastModifiedOn { get; set; }
    public Guid CreatedById { get; set; }
    public Guid? LastModifiedById { get; set; }
}

// --- Request Validator (No change here) ---
public class UpdateIncomeRequestValidator : Validator<UpdateIncomeRequest> {
    public UpdateIncomeRequestValidator() {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Income ID is required.");

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

public class UpdateIncome(AppDbContext dbContext) : Endpoint<UpdateIncomeRequest, UpdateIncomeResponse> {
    public override void Configure() {
        Put("incomes/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.Incomes_Update));
        Summary(s => {
            s.Summary = "Updates an existing income record";
            s.Description = "Updates the details of an existing income entry, including amount, currency (by ID), date, status, description, and income type.";
            s.ExampleRequest = new UpdateIncomeRequest {
                Id = Guid.NewGuid(),
                Amount = 1250.75m,
                CurrencyId = Guid.NewGuid(),
                Date = DateTime.UtcNow.Date.AddDays(-1),
                IncomeStatus = IncomeStatus.Received,
                Description = "Revised client project payment for Q3",
                IncomeTypeId = Guid.NewGuid()
            };
            s.ResponseExamples[200] = new UpdateIncomeResponse {
                Id = Guid.NewGuid(),
                Amount = 1250.75m,
                CurrencyId = Guid.NewGuid(),
                CurrencyName = "US Dollar",
                CurrencyCode = "USD", // Updated example
                Date = DateTime.UtcNow.Date.AddDays(-1),
                IncomeStatus = IncomeStatus.Received,
                Description = "Revised client project payment for Q3",
                IncomeTypeId = Guid.NewGuid(),
                IncomeTypeName = "Project Income",
                CreatedOn = DateTime.UtcNow.AddDays(-10),
                LastModifiedOn = DateTime.UtcNow,
                CreatedById = Guid.NewGuid(),
                LastModifiedById = Guid.NewGuid()
            };
        });
    }

    public override async Task HandleAsync(UpdateIncomeRequest req, CancellationToken ct) {
        var income = await dbContext.Incomes
            .Include(i => i.IncomeType)
            .Include(i => i.Currency)
            .FirstOrDefaultAsync(i => i.Id == req.Id, ct);

        if (income == null) {
            await SendNotFoundAsync(ct);
            return;
        }

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

        income.Amount = req.Amount;
        income.CurrencyId = req.CurrencyId;
        income.Date = req.Date.Date;
        income.IncomeStatus = req.IncomeStatus;
        income.Description = req.Description;
        income.IncomeTypeId = req.IncomeTypeId;
        income.LastModifiedOn = DateTime.UtcNow;
        income.LastModifiedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendOkAsync(new UpdateIncomeResponse {
            Id = income.Id,
            Amount = income.Amount,
            CurrencyId = income.CurrencyId,
            CurrencyName = currency.Name,
            CurrencyCode = currency.Code, // Populate Currency Code
            Date = income.Date,
            IncomeStatus = income.IncomeStatus,
            Description = income.Description,
            IncomeTypeId = income.IncomeTypeId,
            IncomeTypeName = incomeType.Name,
            CreatedOn = income.CreatedOn,
            LastModifiedOn = income.LastModifiedOn,
            CreatedById = income.CreatedById,
            LastModifiedById = income.LastModifiedById
        }, ct);
    }
}