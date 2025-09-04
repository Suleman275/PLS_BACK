using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.Communitites;

public class DeleteCommunityRequest {
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
}

public class DeleteCommunityRequestValidator : Validator<DeleteCommunityRequest> {
    public DeleteCommunityRequestValidator() {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Community ID is required.");
    }
}

public class DeleteCommunity(AppDbContext dbContext) : Endpoint<DeleteCommunityRequest> {
    public override void Configure() {
        Delete("communities/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.Communities_Delete));
    }

    public override async Task HandleAsync(DeleteCommunityRequest req, CancellationToken ct) {
        var community = await dbContext.Communities.FirstOrDefaultAsync(c => c.Id == req.Id, ct);

        if (community == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        community.DeletedOn = DateTime.UtcNow;
        community.DeletedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendNoContentAsync(ct);
    }
}