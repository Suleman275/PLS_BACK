using FastEndpoints;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.UniversityTypes;

public class DeleteUniversityRequest {
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }

}

public class DeleteUniversityType(AppDbContext dbContext) : Endpoint<DeleteUniversityRequest, EmptyResponse> {
    public override void Configure() {
        Delete("university-types/{id}");
        Version(1);
        Permissions(nameof(UserPermission.UniversityTypes_Delete));
    }

    public override async Task HandleAsync(DeleteUniversityRequest req, CancellationToken ct) {
        var universityType = await dbContext.UniversityTypes.FirstOrDefaultAsync(ut => ut.Id == req.Id, ct);

        if (universityType is null) {
            await SendNotFoundAsync(ct);
            return;
        }

        universityType.DeletedOn = DateTime.UtcNow;
        universityType.DeletedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendNoContentAsync(ct);
    }
}
