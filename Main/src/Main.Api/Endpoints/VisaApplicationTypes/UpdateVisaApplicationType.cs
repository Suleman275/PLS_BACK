using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.VisaApplicationTypes;

public class UpdateVisaApplicationTypeRequest {
    [FromRoute]
    public Guid Id { get; set; }

    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }

    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class UpdateVisaApplicationTypeResponse {
    public Guid Id { get; set; }
    public string Message { get; set; } = "Visa Application Type Updated Successfully";
}

public class UpdateVisaApplicationTypeRequestValidator : AbstractValidator<UpdateVisaApplicationTypeRequest> {
    public UpdateVisaApplicationTypeRequestValidator() {
        RuleFor(vat => vat.Id)
            .NotEmpty().WithMessage("Visa Application Type ID is required.")
            .NotNull().WithMessage("Visa Application Type ID cannot be null."); // Ensure the ID from route is valid

        RuleFor(vat => vat.Name)
            .NotEmpty().WithMessage("Name cannot be empty.")
            .MaximumLength(250).WithMessage("Name cannot exceed 250 characters."); // Optional: Add a max length
    }
}

public class UpdateVisaApplicationType(AppDbContext dbContext) : Endpoint<UpdateVisaApplicationTypeRequest, UpdateVisaApplicationTypeResponse> {
    public override void Configure() {
        Put("visa-application-types/{id}");
        Version(1);
        Permissions(nameof(UserPermission.VisaApplicationTypes_Update));
    }

    public override async Task HandleAsync(UpdateVisaApplicationTypeRequest req, CancellationToken ct) {
        var visaApplicationType = await dbContext.VisaApplicationTypes.FindAsync(new object?[] { req.Id }, cancellationToken: ct);

        if (visaApplicationType is null) {
            await SendNotFoundAsync(ct);
            return;
        }

        visaApplicationType.Name = req.Name;
        visaApplicationType.Description = req.Description;
        visaApplicationType.LastModifiedOn = DateTime.UtcNow;
        visaApplicationType.LastModifiedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        var res = new UpdateVisaApplicationTypeResponse {
            Id = visaApplicationType.Id,
            Message = "Visa Application Type Updated Successfully"
        };
        await SendOkAsync(res, ct);
    }
}