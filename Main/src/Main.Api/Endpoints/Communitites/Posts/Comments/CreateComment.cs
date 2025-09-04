using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Communitites.Posts.Comments;

public class CreateCommentRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    [FromRoute]
    public Guid CommunityId { get; set; }
    [FromRoute]
    public Guid PostId { get; set; }
    public string? Content { get; set; }
}

public class CreateCommentResponse {
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public string? Content { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class CreateCommentRequestValidator : Validator<CreateCommentRequest> {
    public CreateCommentRequestValidator() {
        RuleFor(x => x.CommunityId)
            .NotEmpty().WithMessage("Community ID is required.");
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("Post ID is required.");
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content is required.")
            .MaximumLength(1000).WithMessage("Comment content cannot exceed 1000 characters.");
    }
}

public class CreateComment(AppDbContext dbContext) : Endpoint<CreateCommentRequest, CreateCommentResponse> {
    public override void Configure() {
        Post("communities/{communityId}/posts/{postId}/comments");
        Version(1);
        Permissions(nameof(UserPermission.Comments_Create));
    }

    public override async Task HandleAsync(CreateCommentRequest req, CancellationToken ct) {
        var postExists = await dbContext.Posts
            .AnyAsync(p => p.Id == req.PostId && p.CommunityId == req.CommunityId && p.DeletedOn == null, ct);

        if (!postExists) {
            await SendNotFoundAsync(ct);
            return;
        }

        var newComment = new Comment {
            PostId = req.PostId,
            Content = req.Content,
            CreatedById = req.SubjectId
        };

        dbContext.Comments.Add(newComment);
        await dbContext.SaveChangesAsync(ct);

        await SendAsync(new CreateCommentResponse {
            Id = newComment.Id,
            PostId = newComment.PostId,
            Content = newComment.Content,
            CreatedOn = newComment.CreatedOn
        }, 201, ct);
    }
}