using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.UniversityApplications;

public class DeleteUniversityApplicationRequest {
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
}

public class DeleteUniversityApplicationRequestValidator : Validator<DeleteUniversityApplicationRequest> {
    public DeleteUniversityApplicationRequestValidator() {
        RuleFor(x => x.Id)
          .NotEmpty().WithMessage("University Application ID is required.");
    }
}

public class DeleteUniversityApplication(AppDbContext dbContext) : Endpoint<DeleteUniversityApplicationRequest, EmptyResponse> {
    public override void Configure() {
        Delete("university-applications/{id}");
        Version(1);
        Permissions(UserPermission.UniversityApplications_Delete.ToString());
    }

    public override async Task HandleAsync(DeleteUniversityApplicationRequest req, CancellationToken ct) {
        var application = await dbContext.UniversityApplications
            .FirstOrDefaultAsync(ua => ua.Id == req.Id, ct);

        if (application is null) {
            await SendNotFoundAsync(ct);
            return;
        }

        application.DeletedOn = DateTime.UtcNow;
        application.DeletedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendNoContentAsync(ct);
    }
}