using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.ImmigrationClients;

public class DeleteImmigrationClientRequest {
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; } 
}

public class DeleteImmigrationClientRequestValidator : Validator<DeleteImmigrationClientRequest> {
    public DeleteImmigrationClientRequestValidator() {
        RuleFor(x => x.Id)
          .NotEmpty().WithMessage("Immigration Client User ID is required.");
    }
}

public class DeleteImmigrationClient(AppDbContext dbContext) : Endpoint<DeleteImmigrationClientRequest, EmptyResponse> {
    public override void Configure() {
        Delete("immigration-clients/{id}");
        Version(1);
        Permissions(nameof(UserPermission.ImmigrationClients_Delete));
    }

    public override async Task HandleAsync(DeleteImmigrationClientRequest req, CancellationToken ct) {
        var client = await dbContext.Users
            .OfType<ImmigrationClientUser>()
            .FirstOrDefaultAsync(u => u.Id == req.Id, ct);

        if (client is null) {
            await SendNotFoundAsync(ct);
            return;
        }

        client.DeletedOn = DateTime.UtcNow;
        client.DeletedById = req.SubjectId;
        client.IsActive = false; 

        await dbContext.SaveChangesAsync(ct);

        await SendNoContentAsync(ct); 
    }
}