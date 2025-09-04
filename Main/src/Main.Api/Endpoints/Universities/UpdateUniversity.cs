using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.Universities;

public class UpdateUniversityRequest {
    [FromRoute]
    public Guid Id { get; set; }
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

public class UpdateUniversityResponse {
    public string Message { get; set; } = "University Updated Successfully";
}

public class UpdateUniversityRequestValidator : Validator<UpdateUniversityRequest> {
    public UpdateUniversityRequestValidator() {
        RuleFor(x => x.Id)
          .NotEmpty().WithMessage("University ID is required.");

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
          .InclusiveBetween(1000, DateTime.UtcNow.Year + 5).When(x => x.YearFounded.HasValue)
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

public class UpdateUniversity(AppDbContext dbContext) : Endpoint<UpdateUniversityRequest, UpdateUniversityResponse> {
    public override void Configure() {
        Put("universities/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.Universities_Update));
        Summary(s => {
            s.Summary = "Updates an existing university record";
            s.Description = "Updates the details of an existing university, identified by its ID.";
            s.ExampleRequest = new UpdateUniversityRequest {
                Id = Guid.NewGuid(),
                Name = "University of Bucharest (Updated)",
                NumOfCampuses = 6,
                TotalStudents = 32000,
                YearFounded = 1864,
                Description = "One of the leading academic centers in Romania, updated.",
                UniversityTypeId = Guid.NewGuid(),
                LocationId = Guid.NewGuid()
            };
        });
    }

    public override async Task HandleAsync(UpdateUniversityRequest req, CancellationToken ct) {
        var university = await dbContext.Universities
            .FirstOrDefaultAsync(u => u.Id == req.Id, ct);

        if (university == null) {
            await SendNotFoundAsync(ct);
            return;
        }

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

        var nameConflict = await dbContext.Universities
            .AnyAsync(u => u.Name.ToLower() == req.Name.ToLower() && u.Id != req.Id && u.DeletedOn == null, ct);

        if (nameConflict) {
            AddError(r => r.Name, "A university with this name already exists.");
            await SendErrorsAsync(409, ct); 
            return;
        }

        university.Name = req.Name;
        university.NumOfCampuses = req.NumOfCampuses;
        university.TotalStudents = req.TotalStudents;
        university.YearFounded = req.YearFounded;
        university.Description = req.Description;
        university.UniversityTypeId = req.UniversityTypeId;
        university.LocationId = req.LocationId;
        university.LastModifiedOn = DateTime.UtcNow;
        university.LastModifiedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendOkAsync(new UpdateUniversityResponse(), ct);
    }
}