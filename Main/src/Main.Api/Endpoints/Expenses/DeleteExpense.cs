using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Enums;

namespace UserManagement.API.Endpoints.Expenses;

public class DeleteExpenseRequest {
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
}

public class DeleteExpenseRequestValidator : Validator<DeleteExpenseRequest> {
    public DeleteExpenseRequestValidator() {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Expense ID is required.");
    }
}

public class DeleteExpense(AppDbContext dbContext) : Endpoint<DeleteExpenseRequest> {
    public override void Configure() {
        Delete("expenses/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.Expenses_Delete));
        Summary(s => {
            s.Summary = "Deletes an expense record";
            s.Description = "Removes an expense record from the system by its ID.";
            s.ExampleRequest = new DeleteExpenseRequest { Id = Guid.NewGuid() };
            s.Responses[204] = "Expense record deleted successfully.";
            s.Responses[404] = "Expense record not found.";
            s.Responses[403] = "Forbidden: User does not have permission or ownership.";
        });
    }

    public override async Task HandleAsync(DeleteExpenseRequest req, CancellationToken ct) {
        var expense = await dbContext.Expenses
                                      .FirstOrDefaultAsync(e => e.Id == req.Id, ct);

        if (expense == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        expense.DeletedOn = DateTime.UtcNow;
        expense.DeletedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendNoContentAsync(ct);
    }
}