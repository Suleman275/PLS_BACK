using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.IncomeType;

public class CreateIncomeTypeRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class CreateIncomeTypeResponse {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class CreateIncomeTypeRequestValidator : Validator<CreateIncomeTypeRequest> {
    public CreateIncomeTypeRequestValidator() {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Income type name is required.")
            .MaximumLength(100).WithMessage("Income type name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}

public class CreateIncomeType(AppDbContext dbContext) : Endpoint<CreateIncomeTypeRequest, CreateIncomeTypeResponse> {
    public override void Configure() {
        Post("income-types");
        Version(1);
        Permissions(nameof(UserPermission.IncomeTypes_Create));
    }

    public override async Task HandleAsync(CreateIncomeTypeRequest req, CancellationToken ct) {
        var newIncomeType = new Models.IncomeType {
            Name = req.Name,
            Description = req.Description,
            CreatedById = req.SubjectId,
        };

        dbContext.IncomeTypes.Add(newIncomeType);

        await dbContext.SaveChangesAsync(ct);

        await SendAsync(new CreateIncomeTypeResponse {
            Id = newIncomeType.Id,
            Name = newIncomeType.Name,
            Description = newIncomeType.Description,
            CreatedOn = newIncomeType.CreatedOn
        }, 201, ct);
    }
}