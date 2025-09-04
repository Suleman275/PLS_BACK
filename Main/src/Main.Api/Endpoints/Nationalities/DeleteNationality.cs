using FastEndpoints;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.Nationalities;

public class DeleteNationalityRequest {
    [FromRoute] 
    public Guid Id { get; set; }

    [FromClaim(ClaimNames.SubjectId)] 
    public Guid SubjectId { get; set; }
}

public class DeleteNationality(AppDbContext dbContext) : Endpoint<DeleteNationalityRequest, EmptyResponse> {
    public override void Configure() {
        Delete("nationalities/{id}"); 
        Version(1); 
        Permissions(nameof(UserPermission.Nationalities_Delete));
    }

    public override async Task HandleAsync(DeleteNationalityRequest req, CancellationToken ct) {
        var nationalityToDelete = await dbContext.Nationalities.FirstOrDefaultAsync(n => n.Id == req.Id, ct);

        if (nationalityToDelete == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        nationalityToDelete.DeletedById = req.SubjectId; 
        nationalityToDelete.DeletedOn = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        await SendNoContentAsync(ct);
    }
}