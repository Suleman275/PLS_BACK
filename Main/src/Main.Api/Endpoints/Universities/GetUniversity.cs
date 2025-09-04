using FastEndpoints;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.Universities;

public class GetUniversityByIdRequest {
    [FromRoute]
    public Guid Id { get; set; }
}

public class UniversityDetailsResponse {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public int? NumOfCampuses { get; set; }
    public int? TotalStudents { get; set; }
    public int? YearFounded { get; set; }
    public string? Description { get; set; }
    public Guid UniversityTypeId { get; set; }
    public string UniversityTypeName { get; set; } = default!; 
    public Guid LocationId { get; set; }
    public string LocationName { get; set; } = default!; 
    public DateTime CreatedOn { get; set; }
    public Guid CreatedById { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public Guid? LastModifiedById { get; set; }
}

public class GetUniversityById(AppDbContext dbContext) : Endpoint<GetUniversityByIdRequest, UniversityDetailsResponse> {
    public override void Configure() {
        Get("universities/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.Universities_Read)); 
        Summary(s => {
            s.Summary = "Get detailed information about a specific university by ID.";
            s.Description = "Retrieves the full details for a university, including its associated type and location.";
            s.ExampleRequest = new GetUniversityByIdRequest { Id = Guid.NewGuid() };
            s.ResponseExamples[200] = new UniversityDetailsResponse {
                Id = Guid.NewGuid(),
                Name = "University of Bucharest",
                NumOfCampuses = 5,
                TotalStudents = 30000,
                YearFounded = 1864,
                Description = "One of the leading academic centers in Romania.",
                UniversityTypeId = Guid.NewGuid(),
                UniversityTypeName = "Public University",
                LocationId = Guid.NewGuid(),
                LocationName = "Bucharest, Romania",
                CreatedOn = DateTime.UtcNow.AddYears(-10),
                CreatedById = Guid.NewGuid(),
                LastModifiedOn = DateTime.UtcNow.AddMonths(-6),
                LastModifiedById = Guid.NewGuid()
            };
        });
    }

    public override async Task HandleAsync(GetUniversityByIdRequest req, CancellationToken ct) {
        var university = await dbContext.Universities
            .Include(u => u.UniversityType)
            .Include(u => u.Location)
            .Where(u => u.Id == req.Id)
            .FirstOrDefaultAsync(ct);

        if (university is null) {
            await SendNotFoundAsync(ct);
            return;
        }

        var res = new UniversityDetailsResponse {
            Id = university.Id,
            Name = university.Name,
            NumOfCampuses = university.NumOfCampuses,
            TotalStudents = university.TotalStudents,
            YearFounded = university.YearFounded,
            Description = university.Description,
            UniversityTypeId = university.UniversityTypeId,
            UniversityTypeName = university.UniversityType?.Name ?? "Unknown Type", 
            LocationId = university.LocationId,
            LocationName = university.Location != null ? (university.Location?.City + ", " + university.Location?.Country) : "Unknown Location", 
            CreatedOn = university.CreatedOn,
            CreatedById = university.CreatedById,
            LastModifiedOn = university.LastModifiedOn,
            LastModifiedById = university.LastModifiedById
        };

        await SendOkAsync(res, ct);
    }
}