using FastEndpoints;
using FluentValidation;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;
using Microsoft.EntityFrameworkCore;
using Main.Api.Data;

namespace UserManagement.API.Endpoints.UniversityPrograms;

public class CreateUniversityProgramRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    [FromClaim(ClaimNames.Permissions)]
    public List<UserPermission> SubjectPermissions { get; set; } = [];

    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int DurationYears { get; set; }
    public bool IsActive { get; set; } = true; 
    public Guid UniversityId { get; set; }
    public Guid ProgramTypeId { get; set; }
}

public class CreateUniversityProgramResponse {
    public Guid Id { get; set; }
    public string Message { get; set; } = "University Program Created Successfully";
}

public class CreateUniversityProgramRequestValidator : Validator<CreateUniversityProgramRequest> {
    public CreateUniversityProgramRequestValidator() {
        RuleFor(x => x.Name)
          .NotEmpty().WithMessage("Program name is required.")
          .MaximumLength(256).WithMessage("Program name cannot exceed 256 characters.");

        RuleFor(x => x.Description)
          .MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.Description))
          .WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.DurationYears)
          .GreaterThanOrEqualTo(1).WithMessage("Duration in years must be at least 1.");

        RuleFor(x => x.UniversityId)
          .NotEmpty().WithMessage("University ID is required.");
    }
}

public class CreateUniversityProgram(AppDbContext dbContext) : Endpoint<CreateUniversityProgramRequest, CreateUniversityProgramResponse> {
    public override void Configure() {
        Post("university-programs");
        Version(1);
        Permissions(nameof(UserPermission.UniversityPrograms_Create)); 
    }

    public override async Task HandleAsync(CreateUniversityProgramRequest req, CancellationToken ct) {
        var universityExists = await dbContext.Universities.AnyAsync(u => u.Id == req.UniversityId, ct);
        if (!universityExists) {
            AddError(r => r.UniversityId, "Invalid University ID or University does not exist.");
            await SendErrorsAsync(400, ct);
            return;
        }

        var programTypeExists = await dbContext.ProgramTypes.AnyAsync(pt => pt.Id == req.ProgramTypeId, ct);
        if (!programTypeExists) {
            AddError(r => r.ProgramTypeId, "Invalid ProgramType ID");
            await SendErrorsAsync(400, ct);
            return;
        }

        var existingProgram = await dbContext.UniversityPrograms
            .AnyAsync(up => up.Name.ToLower() == req.Name.ToLower() && up.UniversityId == req.UniversityId, ct);

        if (existingProgram) {
            AddError(r => r.Name, $"A program with the name '{req.Name}' already exists for this university.");
            await SendErrorsAsync(409, ct); 
            return;
        }

        var newProgram = new UniversityProgram {
            Name = req.Name,
            Description = req.Description,
            DurationYears = req.DurationYears,
            IsActive = req.IsActive,
            UniversityId = req.UniversityId,
            ProgramTypeId = req.ProgramTypeId,
            CreatedById = req.SubjectId
        };

        dbContext.UniversityPrograms.Add(newProgram);
        await dbContext.SaveChangesAsync(ct);

        await SendAsync(new CreateUniversityProgramResponse {
            Id = newProgram.Id,
        }, 201, ct);
    }
}