using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.IncomeTypes;

public class UpdateIncomeTypeRequest {
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class UpdateIncomeTypeResponse {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class UpdateIncomeTypeRequestValidator : Validator<UpdateIncomeTypeRequest> {
    public UpdateIncomeTypeRequestValidator() {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Income type ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Income type name is required.")
            .MaximumLength(100).WithMessage("Income type name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}
public class UpdateIncomeType(AppDbContext dbContext) : Endpoint<UpdateIncomeTypeRequest, UpdateIncomeTypeResponse> {
    public override void Configure() {
        Put("income-types/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.IncomeTypes_Update));
    }

    public override async Task HandleAsync(UpdateIncomeTypeRequest req, CancellationToken ct) {
        var incomeType = await dbContext.IncomeTypes.FirstOrDefaultAsync(it => it.Id == req.Id, ct);

        if (incomeType == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        incomeType.Name = req.Name;
        incomeType.Description = req.Description;
        incomeType.LastModifiedOn = DateTime.UtcNow;
        incomeType.LastModifiedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendOkAsync(new UpdateIncomeTypeResponse {
            Id = incomeType.Id,
            Name = incomeType.Name,
            Description = incomeType.Description,
        }, ct);
    }
}