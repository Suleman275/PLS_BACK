using FastEndpoints;
using FluentValidation;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;
using Microsoft.EntityFrameworkCore;
using Main.Api.Data;

namespace UserManagement.API.Endpoints.Universities;

public class CreateUniversityRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; } 

    public string Name { get; set; } = default!;
    public int? NumOfCampuses { get; set; }
    public int? TotalStudents { get; set; }
    public int? YearFounded { get; set; }
    public string? Description { get; set; }
    public Guid UniversityTypeId { get; set; }
    public Guid LocationId { get; set; }
}

public class CreateUniversityResponse {
    public Guid Id { get; set; }
    public string Message { get; set; } = "University Created Successfully";
}

public class CreateUniversityRequestValidator : Validator<CreateUniversityRequest> {
    public CreateUniversityRequestValidator() {
        RuleFor(x => x.Name)
          .NotEmpty().WithMessage("University name is required.")
          .MaximumLength(256).WithMessage("University name cannot exceed 256 characters.");

        RuleFor(x => x.NumOfCampuses)
          .GreaterThanOrEqualTo(0).When(x => x.NumOfCampuses.HasValue)
          .WithMessage("Number of campuses cannot be negative.");

        RuleFor(x => x.TotalStudents)
          .GreaterThanOrEqualTo(0).When(x => x.TotalStudents.HasValue)
          .WithMessage("Total students cannot be negative.");

        RuleFor(x => x.YearFounded)
          .InclusiveBetween(1000, DateTime.UtcNow.Year + 5).When(x => x.YearFounded.HasValue) // Allow a few years into the future for planning
          .WithMessage($"Year founded must be between 1000 and {DateTime.UtcNow.Year + 5}.");

        RuleFor(x => x.Description)
          .MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.Description))
          .WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.UniversityTypeId)
          .NotEmpty().WithMessage("University type ID is required.");

        RuleFor(x => x.LocationId)
          .NotEmpty().WithMessage("Location ID is required.");
    }
}

public class CreateUniversity(AppDbContext dbContext) : Endpoint<CreateUniversityRequest, CreateUniversityResponse> {
    public override void Configure() {
        Post("universities");
        Version(1);
        Permissions(nameof(UserPermission.Universities_Create)); 
        Summary(s => {
            s.Summary = "Creates a new university record";
            s.Description = "Adds a new university to the system with its details and associated type and location.";
            s.ExampleRequest = new CreateUniversityRequest {
                Name = "Transylvania University",
                NumOfCampuses = 3,
                TotalStudents = 15000,
                YearFounded = 1948,
                Description = "A historical university in the heart of Transylvania.",
                UniversityTypeId = Guid.NewGuid(), 
                LocationId = Guid.NewGuid() 
            };
            s.ResponseExamples[201] = new CreateUniversityResponse {
                Id = Guid.NewGuid(),
                Message = "University Created Successfully"
            };
        });
    }

    public override async Task HandleAsync(CreateUniversityRequest req, CancellationToken ct) {
        // Validate UniversityType and Location exist
        var universityTypeExists = await dbContext.UniversityTypes.AnyAsync(ut => ut.Id == req.UniversityTypeId, ct);
        if (!universityTypeExists) {
            AddError(r => r.UniversityTypeId, "Invalid University Type ID.");
            await SendErrorsAsync(400, ct);
            return;
        }

        var locationExists = await dbContext.Locations.AnyAsync(l => l.Id == req.LocationId, ct);
        if (!locationExists) {
            AddError(r => r.LocationId, "Invalid Location ID.");
            await SendErrorsAsync(400, ct);
            return;
        }

        var existingUniversity = await dbContext.Universities
            .AnyAsync(u => u.Name.ToLower() == req.Name.ToLower(), ct);

        if (existingUniversity) {
            AddError(r => r.Name, "A university with this name already exists.");
            await SendErrorsAsync(409, ct); // Conflict
            return;
        }

        var newUniversity = new University {
            Name = req.Name,
            NumOfCampuses = req.NumOfCampuses,
            TotalStudents = req.TotalStudents,
            YearFounded = req.YearFounded,
            Description = req.Description,
            UniversityTypeId = req.UniversityTypeId,
            LocationId = req.LocationId,
            CreatedById = req.SubjectId
        };

        dbContext.Universities.Add(newUniversity);
        await dbContext.SaveChangesAsync(ct);

        await SendOkAsync(new CreateUniversityResponse {
            Id = newUniversity.Id,
        }, ct);
    }
}