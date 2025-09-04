using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.UniversityTypes;

public class UpdateUniversityTypeRequest {
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class UpdateUniversityTypeResponse {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class UpdateUniversityTypeValidator : Validator<UpdateUniversityTypeRequest> {
    public UpdateUniversityTypeValidator() {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID is required for update.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");
    }
}

public class UpdateUniversityType(AppDbContext dbContext) : Endpoint<UpdateUniversityTypeRequest, UpdateUniversityTypeResponse> {
    public override void Configure() {
        Put("university-types/{id}");
        Version(1);
        Permissions(nameof(UserPermission.UniversityTypes_Update));
    }

    public override async Task HandleAsync(UpdateUniversityTypeRequest req, CancellationToken ct) {
        var universityType = await dbContext.UniversityTypes.FirstOrDefaultAsync(ut => ut.Id == req.Id, ct);

        if (universityType is null) {
            await SendNotFoundAsync(ct);
            return;
        }

        universityType.Name = req.Name;
        universityType.Description = req.Description;
        universityType.LastModifiedById = req.SubjectId;
        universityType.LastModifiedOn = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        await SendAsync(new UpdateUniversityTypeResponse {
            Id = universityType.Id,
            Name = universityType.Name,
            Description = universityType.Description
        }, cancellation: ct);
    }
}