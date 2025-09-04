using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.UniversityPrograms;

public class ListUniversityProgramsRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }

    [QueryParam]
    public int PageNumber { get; set; } = 1;

    [QueryParam]
    public int PageSize { get; set; } = 10;

    [QueryParam]
    public string? SearchTerm { get; set; }

    [QueryParam]
    public Guid? UniversityId { get; set; }

    [QueryParam]
    public bool? IsActive { get; set; }

    [QueryParam]
    public int? MinDurationYears { get; set; }

    [QueryParam]
    public int? MaxDurationYears { get; set; }
}

public class UniversityProgramListItem {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public int DurationYears { get; set; }
    public bool IsActive { get; set; }
    public Guid UniversityId { get; set; }
    public string UniversityName { get; set; } = default!;
    public Guid ProgramTypeId { get; set; }
    public string ProgramTypeName { get; set; } = default!;
    public DateTime CreatedOn { get; set; }
}

public class ListUniversityProgramsResponse {
    public IEnumerable<UniversityProgramListItem> Programs { get; set; } = Enumerable.Empty<UniversityProgramListItem>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ListUniversityProgramsRequestValidator : Validator<ListUniversityProgramsRequest> {
    public ListUniversityProgramsRequestValidator() {
        RuleFor(x => x.PageNumber)
          .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
          .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.MinDurationYears)
          .GreaterThanOrEqualTo(1).When(x => x.MinDurationYears.HasValue)
          .WithMessage("Minimum duration must be at least 1 year.");

        RuleFor(x => x.MaxDurationYears)
          .GreaterThanOrEqualTo(1).When(x => x.MaxDurationYears.HasValue)
          .WithMessage("Maximum duration must be at least 1 year.")
          .GreaterThanOrEqualTo(x => x.MinDurationYears)
          .When(x => x.MinDurationYears.HasValue && x.MaxDurationYears.HasValue)
          .WithMessage("Maximum duration must be greater than or equal to minimum duration.");
    }
}

public class ListUniversityPrograms(AppDbContext dbContext) : Endpoint<ListUniversityProgramsRequest, ListUniversityProgramsResponse> {
    public override void Configure() {
        Get("university-programs");
        Version(1);
        Permissions(nameof(UserPermission.UniversityPrograms_Read));
    }

    public override async Task HandleAsync(ListUniversityProgramsRequest req, CancellationToken ct) {
        var query = dbContext.UniversityPrograms
            .Include(up => up.University)
            .Include(up => up.ProgramType)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.SearchTerm)) {
            var searchTermLower = req.SearchTerm.ToLower();
            query = query.Where(up =>
              up.Name.ToLower().Contains(searchTermLower) ||
              (up.Description != null && up.Description.ToLower().Contains(searchTermLower)));
        }

        if (req.UniversityId.HasValue && req.UniversityId != Guid.Empty) {
            query = query.Where(up => up.UniversityId == req.UniversityId.Value);
        }

        if (req.IsActive.HasValue) {
            query = query.Where(up => up.IsActive == req.IsActive.Value);
        }

        if (req.MinDurationYears.HasValue) {
            query = query.Where(up => up.DurationYears >= req.MinDurationYears.Value);
        }

        if (req.MaxDurationYears.HasValue) {
            query = query.Where(up => up.DurationYears <= req.MaxDurationYears.Value);
        }

        var totalCount = await query.CountAsync(ct);

        query = query.OrderBy(up => up.Name);

        var programs = await query
          .Skip((req.PageNumber - 1) * req.PageSize)
          .Take(req.PageSize)
          .Select(up => new UniversityProgramListItem {
              Id = up.Id,
              Name = up.Name,
              DurationYears = up.DurationYears,
              IsActive = up.IsActive,
              UniversityId = up.UniversityId,
              UniversityName = up.University.Name,
              ProgramTypeId = up.ProgramTypeId,
              ProgramTypeName = up.ProgramType.Name,
              CreatedOn = up.CreatedOn
          })
          .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling((double)totalCount / req.PageSize);

        await SendOkAsync(new ListUniversityProgramsResponse {
            Programs = programs,
            TotalCount = totalCount,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
            TotalPages = totalPages
        }, ct);
    }
}