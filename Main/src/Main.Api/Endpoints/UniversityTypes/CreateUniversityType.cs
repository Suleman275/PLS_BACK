using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.UniversityTypes;

public class CreateUniversityTypeRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class CreateUniversityTypeResponse {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class CreateUniversityTypeValidator : Validator<CreateUniversityTypeRequest> {
    public CreateUniversityTypeValidator() {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");
    }
}

public class CreateUniversityType(AppDbContext dbContext) : Endpoint<CreateUniversityTypeRequest, CreateUniversityTypeResponse> {
    public override void Configure() {
        Post("university-types");
        Version(1);
        Permissions(nameof(UserPermission.UniversityTypes_Create));
    }

    public override async Task HandleAsync(CreateUniversityTypeRequest req, CancellationToken ct) {
        var universityType = new UniversityType {
            Name = req.Name,
            Description = req.Description,
            CreatedById = req.SubjectId
        };

        dbContext.UniversityTypes.Add(universityType);
        await dbContext.SaveChangesAsync(ct);

        await SendCreatedAtAsync("GetUniversityTypeById", new { Id = universityType.Id }, new CreateUniversityTypeResponse {
            Id = universityType.Id,
            Name = universityType.Name,
            Description = universityType.Description
        }, cancellation: ct);
    }
}