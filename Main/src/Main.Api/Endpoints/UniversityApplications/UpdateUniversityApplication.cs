using FastEndpoints;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.UniversityApplications;

public class UpdateUniversityApplicationRequest {
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    [FromClaim(ClaimNames.Permissions)]
    public List<UserPermission> SubjectPermissions { get; set; } = [];
    public DateTime? ApplyDate { get; set; }
    public DateTime? SubmissionDate { get; set; }
    public DateTime? ResultDate { get; set; }
    public DateTime? ReviewSuccessDate { get; set; }
}

public class UpdateUniversityApplicationResponse {
    public string Message { get; set; } = "University Application Updated Successfully";
}

public class UpdateUniversityApplication(AppDbContext dbContext) : Endpoint<UpdateUniversityApplicationRequest, UpdateUniversityApplicationResponse> {
    public override void Configure() {
        Put("university-applications/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.UniversityApplications_Update), nameof(UserPermission.Students_Own_UniversityApplications_Create));
    }

    public override async Task HandleAsync(UpdateUniversityApplicationRequest req, CancellationToken ct) {
        var application = await dbContext.UniversityApplications
            .Include(ua => ua.Applicant)
            .FirstOrDefaultAsync(ua => ua.Id == req.Id, ct);

        if (application == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        var hasCreatePermission = req.SubjectPermissions.Contains(UserPermission.UniversityApplications_Update);
        var hasAssignedCreatePermission =
            req.SubjectPermissions.Contains(UserPermission.Students_Own_UniversityApplications_Update);

        var applicant = application.Applicant;

        if (!hasCreatePermission && hasAssignedCreatePermission) {
            var isAssigned = false;

            if (applicant is StudentUser student) {
                isAssigned = student.AdmissionAssociateId == req.SubjectId ||
                  student.CounselorId == req.SubjectId ||
                  student.SopWriterId == req.SubjectId;
            }
            else if (applicant is ImmigrationClientUser immigrationClient) {
                isAssigned = immigrationClient.AdmissionAssociateId == req.SubjectId ||
                  immigrationClient.CounselorId == req.SubjectId ||
                  immigrationClient.SopWriterId == req.SubjectId;
            }

            if (!isAssigned) {
                await SendForbiddenAsync(ct);
                return;
            }
        }

        application.ApplyDate = req.ApplyDate ?? application.ApplyDate;
        application.ReviewSuccessDate = req.ReviewSuccessDate;
        application.SubmissionDate = req.SubmissionDate;
        application.ResultDate = req.ResultDate;
        application.LastModifiedOn = DateTime.UtcNow;
        application.LastModifiedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendOkAsync(new UpdateUniversityApplicationResponse(), ct);
    }
}