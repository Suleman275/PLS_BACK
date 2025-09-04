using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.UniversityApplications;

public class CreateUniversityApplicationRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    [FromClaim(ClaimNames.Permissions)]
    public List<UserPermission> SubjectPermissions { get; set; } = [];
    public Guid ApplicantId { get; set; }
    public Guid UniversityProgramId { get; set; }
    public DateTime? ApplyDate { get; set; }
    public DateTime? ReviewSuccessDate { get; set; }
    public DateTime? SubmissionDate { get; set; }
    public DateTime? ResultDate { get; set; }
}

public class CreateUniversityApplicationResponse {
    public Guid Id { get; set; }
    public string Message { get; set; } = "University Application Created Successfully";
}

public class CreateUniversityApplicationRequestValidator : Validator<CreateUniversityApplicationRequest> {
    public CreateUniversityApplicationRequestValidator() {
        RuleFor(x => x.ApplicantId)
          .NotEmpty().WithMessage("Applicant ID is required.");

        RuleFor(x => x.UniversityProgramId)
          .NotEmpty().WithMessage("University Program ID is required.");
    }
}

public class CreateUniversityApplication(AppDbContext dbContext) : Endpoint<CreateUniversityApplicationRequest, CreateUniversityApplicationResponse> {
    public override void Configure() {
        Post("university-applications");
        Version(1);
        Permissions(nameof(UserPermission.UniversityApplications_Create), nameof(UserPermission.Students_Own_UniversityApplications_Create));
    }

    public override async Task HandleAsync(CreateUniversityApplicationRequest req, CancellationToken ct) {
        var applicant = await dbContext.Users
            .Where(u => u.Id == req.ApplicantId)
            .FirstOrDefaultAsync(ct);

        if (applicant == null || applicant.Role != UserRole.Student) {
            AddError(r => r.ApplicantId, "Invalid Applicant ID or applicant is not a student.");
            await SendErrorsAsync(400, ct);
            return;
        }

        var hasCreatePermission = req.SubjectPermissions.Contains(UserPermission.UniversityApplications_Create);
        var hasAssignedCreatePermission =
            req.SubjectPermissions.Contains(UserPermission.Students_Own_UniversityApplications_Create);

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

        var programExists = await dbContext.UniversityPrograms.AnyAsync(up => up.Id == req.UniversityProgramId, ct);
        if (!programExists) {
            AddError(r => r.UniversityProgramId, "Invalid University Program ID or program does not exist.");
            await SendErrorsAsync(400, ct);
            return;
        }

        var existingApplication = await dbContext.UniversityApplications
            .AnyAsync(ua => ua.ApplicantId == req.ApplicantId && ua.UniversityProgramId == req.UniversityProgramId, ct);

        if (existingApplication) {
            AddError(r => r.UniversityProgramId, "An application for this program by this applicant already exists.");
            await SendErrorsAsync(409, ct);
            return;
        }

        var newApplication = new UniversityApplication {
            ApplicantId = req.ApplicantId,
            UniversityProgramId = req.UniversityProgramId,
            ApplicationStatus = ApplicationStatus.UnderReview,
            ApplyDate = req.ApplyDate ?? DateTime.UtcNow,
            ReviewSuccessDate = req.ReviewSuccessDate,
            SubmissionDate = req.SubmissionDate,
            ResultDate = req.ResultDate,
            CreatedById = req.SubjectId
        };

        dbContext.UniversityApplications.Add(newApplication);
        await dbContext.SaveChangesAsync(ct);

        await SendAsync(new CreateUniversityApplicationResponse {
            Id = newApplication.Id,
        }, 201, ct);
    }
}