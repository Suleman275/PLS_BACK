using FastEndpoints;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.VisaApplicationTypes;

public class DeleteVisaApplicationRequest {
    [FromRoute]
    public string Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
}

public class DeleteVisaApplicationType(AppDbContext dbContext) : Endpoint<DeleteVisaApplicationRequest> {
    public override void Configure() {
        Delete("visa-application-types/{id}");
        Version(1);
        Permissions(nameof(UserPermission.VisaApplicationTypes_Delete));
    }

    public override async Task HandleAsync(DeleteVisaApplicationRequest req, CancellationToken ct) {
        var vat = await dbContext.VisaApplicationTypes.FindAsync(new object?[] { req.Id }, cancellationToken: ct);
        if (vat is null) {
            await SendNotFoundAsync(ct);
            return;
        }

        vat.DeletedOn = DateTime.UtcNow;
        vat.DeletedById = req.SubjectId;

        await dbContext.SaveChangesAsync();

        await SendNoContentAsync(ct);
    }
}
