using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Enums;

namespace UserManagement.API.Endpoints.VisaApplications;

public class UpdateVisaApplicationRequest {
    [FromRoute]
    public Guid Id { get; set; }

    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }

    public Guid? VisaApplicationTypeId { get; set; }
    public ApplicationStatus? ApplicationStatus { get; set; }
    public DateTime? ApplyDate { get; set; }
    public DateTime? ReviewSuccessDate { get; set; }
    public DateTime? SubmissionDate { get; set; }
    public DateTime? ResultDate { get; set; }
    public string? Notes { get; set; }
}

public class UpdateVisaApplicationResponse {
    public Guid Id { get; set; }
    public string Message { get; set; } = "Visa Application Updated Successfully";
}

public class UpdateVisaApplicationRequestValidator : Validator<UpdateVisaApplicationRequest> {
    public UpdateVisaApplicationRequestValidator() {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Visa Application ID is required for update.");

        // Validation for the newly included VisaApplicationTypeId
        //RuleFor(x => x.VisaApplicationTypeId)
        //    .NotEmpty().When(x => x.VisaApplicationTypeId.HasValue) // If provided, it must not be empty Guid
        //    .WithMessage("Visa Application Type ID cannot be an empty GUID if provided.");

        RuleFor(x => x.ApplicationStatus)
            .IsInEnum().When(x => x.ApplicationStatus.HasValue)
            .WithMessage("Invalid Application Status value.");

        //RuleFor(x => x.Notes)
        //    .MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.Notes))
        //    .WithMessage("Notes cannot exceed 1000 characters.");
    }
}

public class UpdateVisaApplication(AppDbContext dbContext) : Endpoint<UpdateVisaApplicationRequest, UpdateVisaApplicationResponse> {
    public override void Configure() {
        Put("visa-applications/{Id}");
        Version(1);
        Permissions(UserPermission.VisaApplications_Update.ToString());
    }

    public override async Task HandleAsync(UpdateVisaApplicationRequest req, CancellationToken ct) {
        var visaApplication = await dbContext.VisaApplications.FindAsync(new object?[] { req.Id }, cancellationToken: ct);

        if (visaApplication is null) {
            await SendNotFoundAsync(ct);
            return;
        }

        if (req.VisaApplicationTypeId.HasValue) {
            var newVisaApplicationTypeId = req.VisaApplicationTypeId.Value;
            var visaApplicationTypeExists = await dbContext.VisaApplicationTypes
                .AnyAsync(vat => vat.Id == newVisaApplicationTypeId, ct);

            if (!visaApplicationTypeExists) {
                AddError("VisaApplicationTypeId", "The provided Visa Application Type ID does not exist or is deleted.");
                await SendErrorsAsync(cancellation: ct);
                return;
            }
            visaApplication.VisaApplicationTypeId = newVisaApplicationTypeId;
        }


        if (req.ApplicationStatus.HasValue) {
            visaApplication.ApplicationStatus = req.ApplicationStatus.Value;
        }

        visaApplication.ApplyDate = req.ApplyDate ?? visaApplication.ApplyDate;
        visaApplication.ReviewSuccessDate = req.ReviewSuccessDate;
        visaApplication.SubmissionDate = req.SubmissionDate;
        visaApplication.ResultDate = req.ResultDate;
        visaApplication.Notes = req.Notes;

        visaApplication.LastModifiedOn = DateTime.UtcNow;
        visaApplication.LastModifiedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        var res = new UpdateVisaApplicationResponse {
            Id = visaApplication.Id,
            Message = "Visa Application Updated Successfully"
        };
        await SendOkAsync(res, ct);
    }
}