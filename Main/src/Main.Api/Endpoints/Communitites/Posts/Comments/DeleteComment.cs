using FastEndpoints;
using FastEndpoints.Security;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.Communitites.Posts.Comments;

public class DeleteCommentRequest {
    [FromRoute]
    public Guid CommunityId { get; set; }
    [FromRoute]
    public Guid PostId { get; set; }
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
}

public class DeleteCommentRequestValidator : Validator<DeleteCommentRequest> {
    public DeleteCommentRequestValidator() {
        RuleFor(x => x.CommunityId)
            .NotEmpty().WithMessage("Community ID is required.");
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("Post ID is required.");
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Comment ID is required.");
    }
}

public class DeleteComment(AppDbContext dbContext) : Endpoint<DeleteCommentRequest> {
    public override void Configure() {
        Delete("communities/{communityId}/posts/{postId}/comments/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.Comments_Delete), nameof(UserPermission.Comments_Own_Delete));
    }

    public override async Task HandleAsync(DeleteCommentRequest req, CancellationToken ct) {
        var comment = await dbContext.Comments
            .FirstOrDefaultAsync(c =>
                c.PostId == req.PostId &&
                c.Id == req.Id, ct);

        if (comment == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        if (comment.CreatedById != req.SubjectId) {
            // If not the owner, check for the general 'delete all' permission
            if (!User.HasPermission(nameof(UserPermission.Comments_Delete))) {
                await SendForbiddenAsync(ct);
                return;
            }
        }

        comment.DeletedOn = DateTime.UtcNow;
        comment.DeletedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendNoContentAsync(ct);
    }
}