using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Enums;

namespace UserManagement.API.Endpoints.UniversityApplications;

public class ListUniversityApplicationsRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }

    [QueryParam]
    public int PageNumber { get; set; } = 1;

    [QueryParam]
    public int PageSize { get; set; } = 10;

    [QueryParam]
    public Guid? ApplicantId { get; set; }

    [QueryParam]
    public ApplicationStatus? Status { get; set; }

    [QueryParam]
    public Guid? UniversityId { get; set; }

    [QueryParam]
    public string? SearchTerm { get; set; }
}

public class UniversityApplicationListItem {
    public Guid Id { get; set; }
    public Guid ApplicantId { get; set; }
    public string ApplicantFullName { get; set; } = default!;
    public string ApplicantEmail { get; set; } = default!;
    public Guid UniversityProgramId { get; set; }
    public string ProgramName { get; set; } = default!;
    public Guid UniversityId { get; set; }
    public string UniversityName { get; set; } = default!;
    public ApplicationStatus ApplicationStatus { get; set; }
    public DateTime? ApplyDate { get; set; }
    public DateTime? ReviewSuccessDate { get; set; }
    public DateTime? SubmissionDate { get; set; }
    public DateTime? ResultDate { get; set; }
}

public class ListUniversityApplicationsResponse {
    public IEnumerable<UniversityApplicationListItem> Applications { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ListUniversityApplications(AppDbContext dbContext) : Endpoint<ListUniversityApplicationsRequest, ListUniversityApplicationsResponse> {
    public override void Configure() {
        Get("university-applications");
        Version(1);
        Permissions(nameof(UserPermission.UniversityApplications_Read));
    }

    public override async Task HandleAsync(ListUniversityApplicationsRequest req, CancellationToken ct) {
        var query = dbContext.UniversityApplications
            .Include(ua => ua.Applicant)
            .Include(ua => ua.UniversityProgram)
                .ThenInclude(up => up.University)
            .AsQueryable();

        if (req.ApplicantId.HasValue) {
            query = query.Where(ua => ua.ApplicantId == req.ApplicantId);
        }

        if (req.Status.HasValue) {
            query = query.Where(ua => ua.ApplicationStatus == req.Status.Value);
        }

        if (req.UniversityId.HasValue && req.UniversityId != Guid.Empty) {
            query = query.Where(ua => ua.UniversityProgram.UniversityId == req.UniversityId.Value);
        }

        if (!string.IsNullOrWhiteSpace(req.SearchTerm)) {
            var searchTermLower = req.SearchTerm.ToLower();
            query = query.Where(ua =>
              ua.Applicant.FirstName.ToLower().Contains(searchTermLower) ||
              ua.Applicant.LastName.ToLower().Contains(searchTermLower) ||
              ua.Applicant.Email.ToLower().Contains(searchTermLower) ||
              ua.UniversityProgram.Name.ToLower().Contains(searchTermLower) ||
              ua.UniversityProgram.University.Name.ToLower().Contains(searchTermLower));
        }

        var totalCount = await query.CountAsync(ct);

        query = query.OrderByDescending(ua => ua.CreatedOn);

        var applications = await query
          .Skip((req.PageNumber - 1) * req.PageSize)
          .Take(req.PageSize)
          .Select(ua => new UniversityApplicationListItem {
              Id = ua.Id,
              ApplicantId = ua.ApplicantId,
              ApplicantFullName = $"{ua.Applicant.FirstName} {ua.Applicant.LastName}",
              ApplicantEmail = ua.Applicant.Email,
              UniversityProgramId = ua.UniversityProgramId,
              ProgramName = ua.UniversityProgram.Name,
              UniversityId = ua.UniversityProgram.UniversityId,
              UniversityName = ua.UniversityProgram.University.Name,
              ApplicationStatus = ua.ApplicationStatus,
              ApplyDate = ua.ApplyDate,
              ReviewSuccessDate = ua.ReviewSuccessDate,
              SubmissionDate = ua.SubmissionDate,
              ResultDate = ua.ResultDate
          })
          .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling((double)totalCount / req.PageSize);

        await SendOkAsync(new ListUniversityApplicationsResponse {
            Applications = applications,
            TotalCount = totalCount,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
            TotalPages = totalPages
        }, ct);
    }
}