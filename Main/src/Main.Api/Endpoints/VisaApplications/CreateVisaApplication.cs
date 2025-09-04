using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.VisaApplications;

public class CreateVisaApplicationRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    [FromClaim(ClaimNames.Permissions)]
    public List<UserPermission> SubjectPermissions { get; set; } = [];

    public Guid VisaApplicationTypeId { get; set; }
    public Guid ApplicantId { get; set; }
    public DateTime? ApplyDate { get; set; }
    public DateTime? ReviewSuccessDate { get; set; }
    public DateTime? SubmissionDate { get; set; }
    public DateTime? ResultDate { get; set; }
    public ApplicationStatus? ApplicationStatus { get; set; }
    public string? Notes { get; set; }
}

public class CreateVisaApplicationResponse {
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class CreateVisaApplicationRequestValidator : Validator<CreateVisaApplicationRequest> {
    public CreateVisaApplicationRequestValidator() {
        RuleFor(x => x.ApplicantId)
            .NotEmpty().WithMessage("Applicant ID is required.");

        RuleFor(x => x.VisaApplicationTypeId)
            .NotEmpty().WithMessage("Visa Application Type ID is required.");
    }
}

public class CreateVisaApplication(AppDbContext dbContext) : Endpoint<CreateVisaApplicationRequest, CreateVisaApplicationResponse> {
    public override void Configure() {
        Post("visa-applications");
        Version(1);
        Permissions(nameof(UserPermission.VisaApplications_Create), nameof(UserPermission.Students_Own_VisaApplications_Create), nameof(UserPermission.ImmigrationClients_Own_VisaApplications_Create));
    }

    public override async Task HandleAsync(CreateVisaApplicationRequest req, CancellationToken ct) {
        var applicant = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == req.ApplicantId, ct);

        if (applicant is null) {    
            AddError(r => r.ApplicantId, "Applicant associated with the current user not found.");
            await SendErrorsAsync(400, ct);
            return;
        }

        var hasCreatePermission = req.SubjectPermissions.Contains(UserPermission.VisaApplications_Create);
        var hasAssignedCreatePermission =
            req.SubjectPermissions.Contains(UserPermission.Students_Own_VisaApplications_Create) ||
            req.SubjectPermissions.Contains(UserPermission.ImmigrationClients_Own_VisaApplications_Create);

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

        var visaApplicationTypeExists = await dbContext.VisaApplicationTypes.AnyAsync(vat => vat.Id == req.VisaApplicationTypeId, ct);
        if (!visaApplicationTypeExists) {
            AddError(r => r.VisaApplicationTypeId, "Visa Application Type with the provided ID does not exist");
            await SendErrorsAsync(400, ct);
            return;
        }

        var newApplication = new VisaApplication {
            ApplicantId = req.ApplicantId,
            VisaApplicationTypeId = req.VisaApplicationTypeId,
            ApplicationStatus = ApplicationStatus.UnderReview,
            ApplyDate = req.ApplyDate ?? DateTime.UtcNow,
            ReviewSuccessDate = req.ReviewSuccessDate,
            SubmissionDate = req.SubmissionDate,
            ResultDate = req.ResultDate,
            CreatedById = req.SubjectId,
            Notes = req.Notes,
        };

        dbContext.VisaApplications.Add(newApplication);

        await dbContext.SaveChangesAsync(ct);

        await SendAsync(new CreateVisaApplicationResponse {
            Id = newApplication.Id,
            Message = "Application Created Successfuly"
        }, 201, ct);
    }
}