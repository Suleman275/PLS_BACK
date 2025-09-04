using FastEndpoints;
using FastEndpoints.Security;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.Communitites.Posts;

public class DeletePostRequest {
    [FromRoute]
    public Guid CommunityId { get; set; }
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
}

public class DeletePostRequestValidator : Validator<DeletePostRequest> {
    public DeletePostRequestValidator() {
        RuleFor(x => x.CommunityId)
            .NotEmpty().WithMessage("Community ID is required.");
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Post ID is required.");
    }
}

public class DeletePost(AppDbContext dbContext) : Endpoint<DeletePostRequest> {
    public override void Configure() {
        Delete("communities/{communityId}/posts/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.Posts_Delete), nameof(UserPermission.Posts_Own_Delete));
    }

    public override async Task HandleAsync(DeletePostRequest req, CancellationToken ct) {
        var post = await dbContext.Posts
            .FirstOrDefaultAsync(p =>
                p.CommunityId == req.CommunityId &&
                p.Id == req.Id, ct);

        if (post == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        if (post.CreatedById != req.SubjectId) {
            if (!User.HasPermission(nameof(UserPermission.Posts_Delete))) {
                await SendForbiddenAsync(ct);
                return;
            }
        }

        post.DeletedOn = DateTime.UtcNow;
        post.DeletedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendNoContentAsync(ct);
    }
}