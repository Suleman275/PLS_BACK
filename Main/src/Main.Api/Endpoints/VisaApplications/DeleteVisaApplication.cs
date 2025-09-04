using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using System.Collections.Specialized;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.VisaApplications;

public class DeleteVisaApplicationRequest {
    [FromRoute]
    public Guid Id { get; set; }

    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }

    [FromClaim(ClaimNames.Permissions)]
    public List<UserPermission> SubjectPermissions { get; set; } = [];
}

public class DeleteVisaApplicationRequestValidator : Validator<DeleteVisaApplicationRequest> {
    public DeleteVisaApplicationRequestValidator() {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Application ID is required for deletion.");
    }
}

public class DeleteVisaApplication(AppDbContext dbContext) : Endpoint<DeleteVisaApplicationRequest> {
    public override void Configure() {
        Delete("visa-applications/{Id}");
        Version(1);
        Permissions(UserPermission.VisaApplications_Delete.ToString(), nameof(UserPermission.ImmigrationClients_Own_VisaApplications_Delete), nameof(UserPermission.Students_Own_VisaApplications_Delete));
    }

    public override async Task HandleAsync(DeleteVisaApplicationRequest req, CancellationToken ct) {
        var applicationToDelete = await dbContext.VisaApplications
            .Include(va => va.Applicant)
            .FirstOrDefaultAsync(va => va.Id == req.Id, ct);

        if (applicationToDelete == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        var hasCreatePermission = req.SubjectPermissions.Contains(UserPermission.UniversityApplications_Create);
        var hasAssignedCreatePermission =
            req.SubjectPermissions.Contains(UserPermission.Students_Own_UniversityApplications_Create);

        var applicant = applicationToDelete.Applicant;

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

        applicationToDelete.DeletedOn = DateTime.UtcNow;
        applicationToDelete.DeletedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendNoContentAsync(ct);
    }
}