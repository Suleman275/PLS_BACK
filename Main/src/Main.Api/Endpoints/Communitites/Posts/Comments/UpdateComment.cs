using FastEndpoints;
using FastEndpoints.Security;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.Communitites.Posts.Comments;

public class UpdateCommentRequest {
    [FromRoute]
    public Guid CommunityId { get; set; }
    [FromRoute]
    public Guid PostId { get; set; }
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    public string Content { get; set; } = default!;
}

public class UpdateCommentResponse {
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public string? Content { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public Guid CreatedById { get; set; }
    public Guid? LastModifiedById { get; set; }
}

public class UpdateCommentRequestValidator : Validator<UpdateCommentRequest> {
    public UpdateCommentRequestValidator() {
        RuleFor(x => x.CommunityId)
            .NotEmpty().WithMessage("Community ID is required.");
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("Post ID is required.");
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Comment ID is required.");
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content is required.")
            .MaximumLength(1000).WithMessage("Comment content cannot exceed 1000 characters.");
    }
}

public class UpdateComment(AppDbContext dbContext) : Endpoint<UpdateCommentRequest, UpdateCommentResponse> {
    public override void Configure() {
        Put("communities/{communityId}/posts/{postId}/comments/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.Comments_Update), nameof(UserPermission.Comments_Own_Update));
    }

    public override async Task HandleAsync(UpdateCommentRequest req, CancellationToken ct) {
        var comment = await dbContext.Comments
            .FirstOrDefaultAsync(c => c.PostId == req.PostId && c.Id == req.Id, ct);

        if (comment == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        if (comment.CreatedById != req.SubjectId) {
            if (!User.HasPermission(nameof(UserPermission.Comments_Update))) {
                await SendForbiddenAsync(ct);
                return;
            }
        }

        comment.Content = req.Content;
        comment.LastModifiedOn = DateTime.UtcNow;
        comment.LastModifiedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendOkAsync(new UpdateCommentResponse {
            Id = comment.Id,
            PostId = comment.PostId,
            Content = comment.Content,
            CreatedOn = comment.CreatedOn,
            LastModifiedOn = comment.LastModifiedOn,
            CreatedById = comment.CreatedById,
            LastModifiedById = comment.LastModifiedById
        }, ct);
    }
}
