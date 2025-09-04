using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.Universities;

public class DeleteUniversityRequest {
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; } 
}

public class DeleteUniversityRequestValidator : Validator<DeleteUniversityRequest> {
    public DeleteUniversityRequestValidator() {
        RuleFor(x => x.Id)
          .NotEmpty().WithMessage("University ID is required.");
    }
}

public class DeleteUniversity(AppDbContext dbContext) : Endpoint<DeleteUniversityRequest, EmptyResponse> {
    public override void Configure() {
        Delete("universities/{id}");
        Version(1);
        Permissions(nameof(UserPermission.Universities_Delete)); 
        Summary(s => {
            s.Summary = "Soft deletes a university record";
            s.Description = "Marks a university record as deleted in the system by its ID.";
            s.ExampleRequest = new DeleteUniversityRequest { Id = Guid.NewGuid() };
            s.Responses[204] = "University record soft deleted successfully.";
            s.Responses[404] = "University record not found.";
            s.Responses[403] = "Forbidden: User does not have permission.";
        });
    }

    public override async Task HandleAsync(DeleteUniversityRequest req, CancellationToken ct) {
        var university = await dbContext.Universities
            .FirstOrDefaultAsync(u => u.Id == req.Id, ct);

        if (university is null) {
            await SendNotFoundAsync(ct);
            return;
        }

        university.DeletedOn = DateTime.UtcNow;
        university.DeletedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendNoContentAsync(ct);
    }
}