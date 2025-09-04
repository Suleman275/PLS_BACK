using FastEndpoints;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.DocumentTypes;

public class DeleteDocumentTypeRequest {
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
}

public class DeleteDocumentType(AppDbContext dbContext) : Endpoint<DeleteDocumentTypeRequest, EmptyResponse> {
    public override void Configure() {
        Delete("document-types/{id}");
        Version(1);
        Permissions(nameof(UserPermission.Currencies_Delete));
    }

    public override async Task HandleAsync(DeleteDocumentTypeRequest req, CancellationToken ct) {
        var documentType = await dbContext.DocumentTypes.FirstOrDefaultAsync(dt => dt.Id == req.Id, ct);

        if (documentType is null) {
            await SendNotFoundAsync(ct);
            return;
        }

        documentType.DeletedOn = DateTime.UtcNow;
        documentType.DeletedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendNoContentAsync(ct);
    }
}