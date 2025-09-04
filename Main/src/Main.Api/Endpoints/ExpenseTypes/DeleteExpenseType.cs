using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.ExpenseTypes;

public class DeleteExpenseTypeRequest {
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
}

public class DeleteExpenseTypeRequestValidator : Validator<DeleteExpenseTypeRequest> {
    public DeleteExpenseTypeRequestValidator() {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Expense type ID is required.");
    }
}

public class DeleteExpenseType(AppDbContext dbContext) : Endpoint<DeleteExpenseTypeRequest> {
    public override void Configure() {
        Delete("expense-types/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.ExpenseTypes_Delete));
    }

    public override async Task HandleAsync(DeleteExpenseTypeRequest req, CancellationToken ct) {
        var expenseType = await dbContext.ExpenseTypes.FirstOrDefaultAsync(et => et.Id == req.Id, ct);

        if (expenseType == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        expenseType.DeletedOn = DateTime.UtcNow;
        expenseType.DeletedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendNoContentAsync(ct);
    }
}