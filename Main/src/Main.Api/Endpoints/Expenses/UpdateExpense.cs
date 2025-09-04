// File: UserManagement.API/Endpoints/Expenses/Update.cs
using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Expenses;

// --- Request DTO (No change here) ---
public class UpdateExpenseRequest {
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    public decimal Amount { get; set; }
    public Guid CurrencyId { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public Guid ExpenseTypeId { get; set; }
}

// --- Response DTO ---
public class UpdateExpenseResponse {
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public Guid CurrencyId { get; set; }
    public string CurrencyName { get; set; } = default!;
    public string CurrencyCode { get; set; } = default!; // New: Currency Code
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public Guid ExpenseTypeId { get; set; }
    public string ExpenseTypeName { get; set; } = default!;
    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public Guid CreatedById { get; set; }
    public Guid? LastModifiedById { get; set; }
}

// --- Request Validator (No change here) ---
public class UpdateExpenseRequestValidator : Validator<UpdateExpenseRequest> {
    public UpdateExpenseRequestValidator() {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Expense ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0.");

        RuleFor(x => x.CurrencyId)
            .NotEmpty().WithMessage("Currency ID is required.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required.")
            .LessThanOrEqualTo(DateTime.UtcNow.AddHours(1)).WithMessage("Date cannot be in the future.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.ExpenseTypeId)
            .NotEmpty().WithMessage("Expense type ID is required.");
    }
}

public class UpdateExpense(AppDbContext dbContext) : Endpoint<UpdateExpenseRequest, UpdateExpenseResponse> {
    public override void Configure() {
        Put("expenses/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.Expenses_Update));
        Summary(s => {
            s.Summary = "Updates an existing expense record";
            s.Description = "Updates the details of an existing expense entry, including amount, currency (by ID), date, description, and expense type.";
            s.ExampleRequest = new UpdateExpenseRequest {
                Id = Guid.NewGuid(),
                Amount = 80.00m,
                CurrencyId = Guid.NewGuid(),
                Date = DateTime.UtcNow.Date.AddDays(-1),
                Description = "Revised lunch with team",
                ExpenseTypeId = Guid.NewGuid()
            };
            s.ResponseExamples[200] = new UpdateExpenseResponse {
                Id = Guid.NewGuid(),
                Amount = 80.00m,
                CurrencyId = Guid.NewGuid(),
                CurrencyName = "US Dollar",
                CurrencyCode = "USD", // Updated example
                Date = DateTime.UtcNow.Date.AddDays(-1),
                Description = "Revised lunch with team",
                ExpenseTypeId = Guid.NewGuid(),
                ExpenseTypeName = "Restaurant",
                CreatedOn = DateTime.UtcNow.AddDays(-10),
                LastModifiedOn = DateTime.UtcNow,
                CreatedById = Guid.NewGuid(),
                LastModifiedById = Guid.NewGuid()
            };
        });
    }

    public override async Task HandleAsync(UpdateExpenseRequest req, CancellationToken ct) {
        var expense = await dbContext.Expenses
            .Include(e => e.ExpenseType)
            .Include(e => e.Currency)
            .FirstOrDefaultAsync(e => e.Id == req.Id, ct);

        if (expense == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        var expenseType = await dbContext.ExpenseTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(et => et.Id == req.ExpenseTypeId, ct);

        if (expenseType == null) {
            AddError(req => req.ExpenseTypeId, "Specified Expense Type does not exist.");
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

        expense.Amount = req.Amount;
        expense.CurrencyId = req.CurrencyId;
        expense.Date = req.Date.Date;
        expense.Description = req.Description;
        expense.ExpenseTypeId = req.ExpenseTypeId;
        expense.LastModifiedOn = DateTime.UtcNow;
        expense.LastModifiedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendOkAsync(new UpdateExpenseResponse {
            Id = expense.Id,
            Amount = expense.Amount,
            CurrencyId = expense.CurrencyId,
            CurrencyName = currency.Name,
            CurrencyCode = currency.Code, // Populate Currency Code
            Date = expense.Date,
            Description = expense.Description,
            ExpenseTypeId = expense.ExpenseTypeId,
            ExpenseTypeName = expenseType.Name,
            CreatedOn = expense.CreatedOn,
            LastModifiedOn = expense.LastModifiedOn,
            CreatedById = expense.CreatedById,
            LastModifiedById = expense.LastModifiedById
        }, ct);
    }
}