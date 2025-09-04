using FastEndpoints;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.ProgramTypes;

public class DeleteProgramTypeRequest {
    [FromRoute]
    public Guid Id { get; set; }

    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
}

public class DeleteProgramType(AppDbContext dbContext) : Endpoint<DeleteProgramTypeRequest, EmptyResponse> {
    public override void Configure() {
        Delete("program-types/{id}");
        Version(1);
        Permissions(nameof(UserPermission.ProgramTypes_Delete));
    }

    public override async Task HandleAsync(DeleteProgramTypeRequest req, CancellationToken ct) {
        var programType = await dbContext.ProgramTypes.FirstOrDefaultAsync(pt => pt.Id == req.Id, ct);

        if (programType is null) {
            await SendNotFoundAsync(ct);
            return;
        }

        programType.DeletedOn = DateTime.UtcNow;
        programType.DeletedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendNoContentAsync(ct);
    }
}