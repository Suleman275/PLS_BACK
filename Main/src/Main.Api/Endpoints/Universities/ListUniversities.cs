using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.Universities;

public class ListUniversitiesRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }

    [QueryParam]
    public int PageNumber { get; set; } = 1;

    [QueryParam]
    public int PageSize { get; set; } = 10;

    [QueryParam]
    public string? SearchTerm { get; set; }

    [QueryParam]
    public Guid? UniversityTypeId { get; set; }

    [QueryParam]
    public Guid? LocationId { get; set; }

    [QueryParam]
    public int? MinStudents { get; set; }

    [QueryParam]
    public int? MaxStudents { get; set; }
}

public class UniversityListItem {
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
}

public class ListUniversitiesResponse {
    public IEnumerable<UniversityListItem> Universities { get; set; } = Enumerable.Empty<UniversityListItem>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ListUniversitiesRequestValidator : Validator<ListUniversitiesRequest> {
    public ListUniversitiesRequestValidator() {
        RuleFor(x => x.PageNumber)
          .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
          .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.MinStudents)
          .GreaterThanOrEqualTo(0).When(x => x.MinStudents.HasValue)
          .WithMessage("Minimum students cannot be negative.");

        RuleFor(x => x.MaxStudents)
          .GreaterThanOrEqualTo(0).When(x => x.MaxStudents.HasValue)
          .WithMessage("Maximum students cannot be negative.")
          .GreaterThanOrEqualTo(x => x.MinStudents)
          .When(x => x.MinStudents.HasValue && x.MaxStudents.HasValue)
          .WithMessage("Maximum students must be greater than or equal to minimum students.");
    }
}

public class ListUniversities(AppDbContext dbContext) : Endpoint<ListUniversitiesRequest, ListUniversitiesResponse> {
    public override void Configure() {
        Get("universities");
        Version(1);
        Permissions(nameof(UserPermission.Universities_Read)); 
        Summary(s => {
            s.Summary = "Lists universities with pagination, search, and filters";
            s.Description = "Retrieves a paginated list of universities, optionally filtered by name, type, location, or student count.";
            s.ExampleRequest = new ListUniversitiesRequest {
                PageNumber = 1,
                PageSize = 5,
                SearchTerm = "tech",
                UniversityTypeId = Guid.NewGuid(), // Placeholder
                MinStudents = 10000,
            };
            s.ResponseExamples[200] = new ListUniversitiesResponse {
                Universities = new[]
                {
                    new UniversityListItem
                    {
                        Id = Guid.NewGuid(),
                        Name = "Politehnica University",
                        NumOfCampuses = 2,
                        TotalStudents = 20000,
                        YearFounded = 1818,
                        Description = "Leading technical university.",
                        UniversityTypeId = Guid.NewGuid(),
                        UniversityTypeName = "Technical University",
                        LocationId = Guid.NewGuid(),
                        LocationName = "Bucharest",
                        CreatedOn = DateTime.UtcNow.AddYears(-5)
                    }
                },
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 5,
                TotalPages = 1
            };
        });
    }

    public override async Task HandleAsync(ListUniversitiesRequest req, CancellationToken ct) {
        var query = dbContext.Universities
            .Include(u => u.UniversityType)
            .Include(u => u.Location)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.SearchTerm)) {
            var searchTermLower = req.SearchTerm.ToLower();
            query = query.Where(u =>
              u.Name.ToLower().Contains(searchTermLower) ||
              (u.Description != null && u.Description.ToLower().Contains(searchTermLower)));
        }

        if (req.UniversityTypeId.HasValue && req.UniversityTypeId != Guid.Empty) {
            query = query.Where(u => u.UniversityTypeId == req.UniversityTypeId.Value);
        }

        if (req.LocationId.HasValue && req.LocationId != Guid.Empty) {
            query = query.Where(u => u.LocationId == req.LocationId.Value);
        }

        if (req.MinStudents.HasValue) {
            query = query.Where(u => u.TotalStudents >= req.MinStudents.Value);
        }

        if (req.MaxStudents.HasValue) {
            query = query.Where(u => u.TotalStudents <= req.MaxStudents.Value);
        }

        var totalCount = await query.CountAsync(ct);

        query = query.OrderBy(u => u.Name);

        var universities = await query
          .Skip((req.PageNumber - 1) * req.PageSize)
          .Take(req.PageSize)
          .Select(u => new UniversityListItem {
              Id = u.Id,
              Name = u.Name,
              NumOfCampuses = u.NumOfCampuses,
              TotalStudents = u.TotalStudents,
              YearFounded = u.YearFounded,
              Description = u.Description,
              UniversityTypeId = u.UniversityTypeId,
              UniversityTypeName = u.UniversityType.Name,
              LocationId = u.LocationId,
              LocationName = u.Location.City + ", " + u.Location.Country,
              CreatedOn = u.CreatedOn
          })
          .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling((double)totalCount / req.PageSize);

        await SendOkAsync(new ListUniversitiesResponse {
            Universities = universities,
            TotalCount = totalCount,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
            TotalPages = totalPages
        }, ct);
    }
}