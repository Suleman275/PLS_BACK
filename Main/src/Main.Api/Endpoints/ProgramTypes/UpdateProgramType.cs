using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.ProgramTypes;

public class UpdateProgramTypeRequest {
    [FromRoute]
    public Guid Id { get; set; }

    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class UpdateProgramTypeResponse {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class UpdateProgramTypeValidator : Validator<UpdateProgramTypeRequest> {
    public UpdateProgramTypeValidator() {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID is required for update.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");
    }
}

public class UpdateProgramType(AppDbContext dbContext) : Endpoint<UpdateProgramTypeRequest, UpdateProgramTypeResponse> {
    public override void Configure() {
        Put("program-types/{id}");
        Version(1);
        Permissions(nameof(UserPermission.ProgramTypes_Read));
    }

    public override async Task HandleAsync(UpdateProgramTypeRequest req, CancellationToken ct) {
        var programType = await dbContext.ProgramTypes.FirstOrDefaultAsync(pt => pt.Id == req.Id, ct);

        if (programType is null) {
            await SendNotFoundAsync(ct);
            return;
        }

        programType.Name = req.Name;
        programType.Description = req.Description;
        programType.LastModifiedOn = DateTime.UtcNow;
        programType.LastModifiedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendAsync(new UpdateProgramTypeResponse {
            Id = programType.Id,
            Name = programType.Name,
            Description = programType.Description
        }, cancellation: ct);
    }
}