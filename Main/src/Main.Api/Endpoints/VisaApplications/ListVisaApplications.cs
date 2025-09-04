using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Enums;

namespace UserManagement.API.Endpoints.VisaApplications;

public class VisaApplicationDto {
    public Guid Id { get; set; }
    public Guid ApplicantId { get; set; }
    public string ApplicantFullName { get; set; } = default!;
    public Guid VisaApplicationTypeId { get; set; }
    public string VisaApplicationTypeName { get; set; } = default!;
    public ApplicationStatus ApplicationStatus { get; set; }
    public DateTime ApplyDate { get; set; }
    public DateTime? ReviewSuccessDate { get; set; }
    public DateTime? SubmissionDate { get; set; }
    public DateTime? ResultDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid? CreatedById { get; set; }
}

public class ListVisaApplicationsRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }

    [QueryParam]
    public int PageNumber { get; set; } = 1;

    [QueryParam]
    public int PageSize { get; set; } = 10;

    [QueryParam]
    public string? SearchTerm { get; set; }

    [QueryParam]
    public Guid? ApplicantId { get; set; }

    [QueryParam]
    public ApplicationStatus? Status { get; set; }
}

public class ListVisaApplicationsResponse {
    public IEnumerable<VisaApplicationDto> VisaApplications { get; set; } = new List<VisaApplicationDto>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class ListVisaApplications(AppDbContext dbContext) : Endpoint<ListVisaApplicationsRequest, ListVisaApplicationsResponse> {
    public override void Configure() {
        Get("visa-applications");
        Version(1);
        Permissions(UserPermission.VisaApplications_Read.ToString());
    }

    public override async Task HandleAsync(ListVisaApplicationsRequest req, CancellationToken ct) {
        var query = dbContext.VisaApplications
            .Include(va => va.Applicant)
            .Include(va => va.VisaApplicationType)
            .AsNoTracking();

        if (req.ApplicantId.HasValue) {
            query = query.Where(va => va.ApplicantId == req.ApplicantId.Value);
        }

        if (req.Status.HasValue) {
            query = query.Where(va => va.ApplicationStatus == req.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(req.SearchTerm)) {
            var searchTermLower = req.SearchTerm.ToLower();
            query = query.Where(va =>
                (va.Notes != null && va.Notes.ToLower().Contains(searchTermLower)) ||
                va.Applicant.FirstName.ToLower().Contains(searchTermLower) || // Search by applicant's first name
                va.Applicant.LastName.ToLower().Contains(searchTermLower) ||  // Search by applicant's last name
                va.VisaApplicationType.Name.ToLower().Contains(searchTermLower) // Search by visa type name
            );
        }

        var totalCount = await query.CountAsync(ct);

        var visaApplications = await query
            .OrderByDescending(va => va.ApplyDate)
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(va => new VisaApplicationDto {
                Id = va.Id,
                ApplicantId = va.ApplicantId,
                ApplicantFullName = $"{va.Applicant.FirstName} {va.Applicant.LastName}".Trim(),
                VisaApplicationTypeId = va.VisaApplicationTypeId,
                VisaApplicationTypeName = va.VisaApplicationType.Name,
                ApplicationStatus = va.ApplicationStatus,
                ApplyDate = va.ApplyDate,
                ReviewSuccessDate = va.ReviewSuccessDate,
                SubmissionDate = va.SubmissionDate,
                ResultDate = va.ResultDate,
                Notes = va.Notes,
                CreatedOn = va.CreatedOn,
                CreatedById = va.CreatedById
            })
            .ToListAsync(ct);

        var response = new ListVisaApplicationsResponse {
            VisaApplications = visaApplications,
            TotalCount = totalCount,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize
        };

        await SendOkAsync(response, ct);
    }
}