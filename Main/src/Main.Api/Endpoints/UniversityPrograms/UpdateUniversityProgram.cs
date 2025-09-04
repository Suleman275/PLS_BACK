using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.UniversityPrograms;

public class UpdateUniversityProgramRequest {
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; } 

    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int DurationYears { get; set; }
    public bool IsActive { get; set; }
    public Guid UniversityId { get; set; }
    public Guid ProgramTypeId { get; set; }
}

public class UpdateUniversityProgramResponse {
    public string Message { get; set; } = "University Program Updated Successfully";
}

public class UpdateUniversityProgramRequestValidator : Validator<UpdateUniversityProgramRequest> {
    public UpdateUniversityProgramRequestValidator() {
        RuleFor(x => x.Id)
          .NotEmpty().WithMessage("University Program ID is required.");

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

public class UpdateUniversityProgram(AppDbContext dbContext) : Endpoint<UpdateUniversityProgramRequest, UpdateUniversityProgramResponse> {
    public override void Configure() {
        Put("university-programs/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.UniversityPrograms_Update));
    }

    public override async Task HandleAsync(UpdateUniversityProgramRequest req, CancellationToken ct) {
        var program = await dbContext.UniversityPrograms
            .FirstOrDefaultAsync(up => up.Id == req.Id, ct);

        if (program == null) {
            await SendNotFoundAsync(ct);
            return;
        }

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

        var nameConflict = await dbContext.UniversityPrograms
            .AnyAsync(up => up.Name.ToLower() == req.Name.ToLower() && up.UniversityId == req.UniversityId && up.Id != req.Id, ct);

        if (nameConflict) {
            AddError(r => r.Name, $"A program with the name '{req.Name}' already exists for this university.");
            await SendErrorsAsync(409, ct);
            return;
        }

        program.Name = req.Name;
        program.Description = req.Description;
        program.DurationYears = req.DurationYears;
        program.IsActive = req.IsActive;
        program.UniversityId = req.UniversityId; 
        program.LastModifiedOn = DateTime.UtcNow;
        program.LastModifiedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendOkAsync(new UpdateUniversityProgramResponse(), ct);
    }
}