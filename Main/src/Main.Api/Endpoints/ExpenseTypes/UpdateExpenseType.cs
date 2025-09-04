using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.ExpenseTypes;

public class UpdateExpenseTypeRequest {
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class UpdateExpenseTypeResponse {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public Guid CreatedById { get; set; }
    public Guid? LastModifiedById { get; set; }
}

public class UpdateExpenseTypeRequestValidator : Validator<UpdateExpenseTypeRequest> {
    public UpdateExpenseTypeRequestValidator() {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Expense type ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Expense type name is required.")
            .MaximumLength(100).WithMessage("Expense type name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}

public class UpdateExpenseType(AppDbContext dbContext) : Endpoint<UpdateExpenseTypeRequest, UpdateExpenseTypeResponse> {
    public override void Configure() {
        Put("expense-types/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.ExpenseTypes_Update));
    }

    public override async Task HandleAsync(UpdateExpenseTypeRequest req, CancellationToken ct) {
        var expenseType = await dbContext.ExpenseTypes.FirstOrDefaultAsync(et => et.Id == req.Id, ct);

        if (expenseType == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        expenseType.Name = req.Name;
        expenseType.Description = req.Description;
        expenseType.LastModifiedOn = DateTime.UtcNow;
        expenseType.LastModifiedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendOkAsync(new UpdateExpenseTypeResponse {
            Id = expenseType.Id,
            Name = expenseType.Name,
            Description = expenseType.Description,
            CreatedOn = expenseType.CreatedOn,
            LastModifiedOn = expenseType.LastModifiedOn,
            CreatedById = expenseType.CreatedById,
            LastModifiedById = expenseType.LastModifiedById
        }, ct);
    }
}
