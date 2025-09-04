using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.ExpenseTypes;

public class CreateExpenseTypeRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class CreateExpenseTypeResponse {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class CreateExpenseTypeRequestValidator : Validator<CreateExpenseTypeRequest> {
    public CreateExpenseTypeRequestValidator() {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Expense type name is required.")
            .MaximumLength(100).WithMessage("Expense type name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}

public class CreateExpenseType(AppDbContext dbContext) : Endpoint<CreateExpenseTypeRequest, CreateExpenseTypeResponse> {
    public override void Configure() {
        Post("expense-types");
        Version(1);
        Permissions(nameof(UserPermission.ExpenseTypes_Create));
    }

    public override async Task HandleAsync(CreateExpenseTypeRequest req, CancellationToken ct) {
        var newExpenseType = new ExpenseType {
            Name = req.Name,
            Description = req.Description,
            CreatedById = req.SubjectId,
        };

        dbContext.ExpenseTypes.Add(newExpenseType);

        await dbContext.SaveChangesAsync(ct);

        await SendAsync(new CreateExpenseTypeResponse {
            Id = newExpenseType.Id,
            Name = newExpenseType.Name,
            Description = newExpenseType.Description,
            CreatedOn = newExpenseType.CreatedOn
        }, 201, ct);
    }
}