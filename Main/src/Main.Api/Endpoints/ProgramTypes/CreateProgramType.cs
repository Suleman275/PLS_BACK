using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.ProgramTypes;

public class CreateProgramTypeRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class CreateProgramTypeResponse {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class CreateProgramTypeValidator : Validator<CreateProgramTypeRequest> {
    public CreateProgramTypeValidator() {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");
    }
}

public class CreateProgramType(AppDbContext dbContext) : Endpoint<CreateProgramTypeRequest, CreateProgramTypeResponse> {
    public override void Configure() {
        Post("program-types");
        Version(1);
        Permissions(nameof(UserPermission.ProgramTypes_Create));
    }

    public override async Task HandleAsync(CreateProgramTypeRequest req, CancellationToken ct) {
        var programType = new ProgramType {
            Name = req.Name,
            Description = req.Description,
            CreatedById = req.SubjectId
        };

        dbContext.ProgramTypes.Add(programType);
        await dbContext.SaveChangesAsync(ct);

        await SendCreatedAtAsync("GetProgramTypeById", new { Id = programType.Id }, new CreateProgramTypeResponse {
            Id = programType.Id,
            Name = programType.Name,
            Description = programType.Description
        }, cancellation: ct);
    }
}