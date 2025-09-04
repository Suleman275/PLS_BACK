using FastEndpoints;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.Locations;

public class DeleteLocationRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }

    [FromRoute]
    public Guid LocationId { get; set; }
}

public class DeleteLocation(AppDbContext dbContext) : Endpoint<DeleteLocationRequest, EmptyResponse> {
    public override void Configure() {
        Delete("locations/{LocationId}");
        Version(1);
        Permissions(nameof(UserPermission.Locations_Delete));
    }

    public override async Task HandleAsync(DeleteLocationRequest req, CancellationToken ct) {

        var locationToDelete = await dbContext.Locations.FindAsync(req.LocationId, ct);

        if (locationToDelete == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        locationToDelete.DeletedById = req.SubjectId;
        locationToDelete.DeletedOn = DateTime.UtcNow;

        dbContext.Locations.Update(locationToDelete);

        await dbContext.SaveChangesAsync(ct);

        await SendNoContentAsync(ct);
    }
}