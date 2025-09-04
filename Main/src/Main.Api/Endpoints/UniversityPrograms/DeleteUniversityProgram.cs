using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.UniversityPrograms;

public class DeleteUniversityProgramRequest {
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; } 
}

public class DeleteUniversityProgramRequestValidator : Validator<DeleteUniversityProgramRequest> {
    public DeleteUniversityProgramRequestValidator() {
        RuleFor(x => x.Id)
          .NotEmpty().WithMessage("University Program ID is required.");
    }
}

public class DeleteUniversityProgram(AppDbContext dbContext) : Endpoint<DeleteUniversityProgramRequest, EmptyResponse> {
    public override void Configure() {
        Delete("university-programs/{id}");
        Version(1);
        Permissions(nameof(UserPermission.UniversityPrograms_Delete)); 
        Summary(s => {
            s.Summary = "Soft deletes a university program record";
            s.Description = "Marks a university program record as deleted in the system by its ID.";
            s.ExampleRequest = new DeleteUniversityProgramRequest { Id = Guid.NewGuid() };
            s.Responses[204] = "University program record soft deleted successfully.";
            s.Responses[404] = "University program record not found.";
            s.Responses[403] = "Forbidden: User does not have permission.";
        });
    }

    public override async Task HandleAsync(DeleteUniversityProgramRequest req, CancellationToken ct) {
        var program = await dbContext.UniversityPrograms
            .FirstOrDefaultAsync(up => up.Id == req.Id && up.DeletedOn == null, ct);

        if (program is null) {
            await SendNotFoundAsync(ct);
            return;
        }

        program.DeletedOn = DateTime.UtcNow;
        program.DeletedById = req.SubjectId;
        program.IsActive = false; 

        await dbContext.SaveChangesAsync(ct);

        await SendNoContentAsync(ct); // 204 No Content typically for successful delete
    }
}