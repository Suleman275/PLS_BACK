using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.ClientSources;

public class DeleteClientSourceRequest {
    [FromRoute]
    public Guid Id { get; set; }

    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
}

public class DeleteClientSourceRequestValidator : Validator<DeleteClientSourceRequest> {
    public DeleteClientSourceRequestValidator() {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Client source ID is required.");
    }
}

public class DeleteClientSource(AppDbContext dbContext) : Endpoint<DeleteClientSourceRequest> {
    public override void Configure() {
        Delete("client-sources/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.ClientSources_Delete));
    }

    public override async Task HandleAsync(DeleteClientSourceRequest req, CancellationToken ct) {
        var clientSource = await dbContext.ClientSources.FirstOrDefaultAsync(cs => cs.Id == req.Id, ct);

        if (clientSource == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        clientSource.DeletedOn = DateTime.UtcNow;
        clientSource.DeletedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendNoContentAsync(ct);
    }
}