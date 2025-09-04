// File: UserManagement.API/Endpoints/Expenses/Create.cs
using FastEndpoints;
using FluentValidation;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;
using Microsoft.EntityFrameworkCore;
using Main.Api.Data;

namespace UserManagement.API.Endpoints.Expenses;

// --- Request DTO (No change here) ---
public class CreateExpenseRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    public decimal Amount { get; set; }
    public Guid CurrencyId { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public Guid ExpenseTypeId { get; set; }
}

// --- Response DTO ---
public class CreateExpenseResponse {
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
}

// --- Request Validator (No change here) ---
public class CreateExpenseRequestValidator : Validator<CreateExpenseRequest> {
    public CreateExpenseRequestValidator() {
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

public class CreateExpense(AppDbContext dbContext) : Endpoint<CreateExpenseRequest, CreateExpenseResponse> {
    public override void Configure() {
        Post("expenses");
        Version(1);
        Permissions(nameof(UserPermission.Expenses_Create));
        Summary(s => {
            s.Summary = "Creates a new expense record";
            s.Description = "Adds a new expense entry to the system with details like amount, currency (by ID), date, description, and expense type.";
            s.ExampleRequest = new CreateExpenseRequest {
                Amount = 75.25m,
                CurrencyId = Guid.NewGuid(),
                Date = DateTime.UtcNow.Date,
                Description = "Lunch with team",
                ExpenseTypeId = Guid.NewGuid()
            };
            s.ResponseExamples[201] = new CreateExpenseResponse {
                Id = Guid.NewGuid(),
                Amount = 75.25m,
                CurrencyId = Guid.NewGuid(),
                CurrencyName = "US Dollar",
                CurrencyCode = "USD", // Updated example
                Date = DateTime.UtcNow.Date,
                Description = "Lunch with team",
                ExpenseTypeId = Guid.NewGuid(),
                ExpenseTypeName = "Restaurant",
                CreatedOn = DateTime.UtcNow
            };
        });
    }

    public override async Task HandleAsync(CreateExpenseRequest req, CancellationToken ct) {
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

        var newExpense = new Expense {
            Amount = req.Amount,
            CurrencyId = req.CurrencyId,
            Date = req.Date.Date,
            Description = req.Description,
            ExpenseTypeId = req.ExpenseTypeId,
            CreatedById = req.SubjectId,
        };

        dbContext.Expenses.Add(newExpense);

        await dbContext.SaveChangesAsync(ct);

        await SendAsync(new CreateExpenseResponse {
            Id = newExpense.Id,
            Amount = newExpense.Amount,
            CurrencyId = newExpense.CurrencyId,
            CurrencyName = currency.Name,
            CurrencyCode = currency.Code, // Populate Currency Code
            Date = newExpense.Date,
            Description = newExpense.Description,
            ExpenseTypeId = newExpense.ExpenseTypeId,
            ExpenseTypeName = expenseType.Name,
            CreatedOn = newExpense.CreatedOn
        }, 201, ct);
    }
}