using FastEndpoints;
using FastEndpoints.Security;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.Communitites.Posts;

public class UpdatePostRequest {
    [FromRoute]
    public Guid CommunityId { get; set; }
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
}

public class UpdatePostResponse {
    public Guid Id { get; set; }
    public Guid CommunityId { get; set; }
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public Guid CreatedById { get; set; }
    public Guid? LastModifiedById { get; set; }
}

public class UpdatePostRequestValidator : Validator<UpdatePostRequest> {
    public UpdatePostRequestValidator() {
        RuleFor(x => x.CommunityId)
            .NotEmpty().WithMessage("Community ID is required.");
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Post ID is required.");
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Post title is required.")
            .MaximumLength(200).WithMessage("Post title cannot exceed 200 characters.");
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Post content is required.")
            .MaximumLength(4000).WithMessage("Post content cannot exceed 4000 characters.");
    }
}

public class UpdatePost(AppDbContext dbContext) : Endpoint<UpdatePostRequest, UpdatePostResponse> {
    public override void Configure() {
        Put("communities/{communityId}/posts/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.Posts_Update), nameof(UserPermission.Posts_Own_Update));
    }

    public override async Task HandleAsync(UpdatePostRequest req, CancellationToken ct) {
        var post = await dbContext.Posts
            .FirstOrDefaultAsync(p =>
                p.CommunityId == req.CommunityId &&
                p.Id == req.Id, ct);

        if (post == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        if (post.CreatedById != req.SubjectId) {
            if (!User.HasPermission(nameof(UserPermission.Posts_Update))) {
                await SendForbiddenAsync(ct);
                return;
            }
        }

        post.Title = req.Title;
        post.Content = req.Content;
        post.LastModifiedOn = DateTime.UtcNow;
        post.LastModifiedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendOkAsync(new UpdatePostResponse {
            Id = post.Id,
            CommunityId = post.CommunityId,
            Title = post.Title,
            Content = post.Content,
            CreatedOn = post.CreatedOn,
            LastModifiedOn = post.LastModifiedOn,
            CreatedById = post.CreatedById,
            LastModifiedById = post.LastModifiedById
        }, ct);
    }
}