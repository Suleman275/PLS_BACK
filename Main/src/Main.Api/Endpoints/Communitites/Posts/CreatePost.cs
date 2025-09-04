using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Communitites.Posts;

public class CreatePostRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    [FromRoute]
    public Guid CommunityId { get; set; }
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
}

public class CreatePostResponse {
    public Guid Id { get; set; }
    public Guid CommunityId { get; set; }
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTime CreatedOn { get; set; }
}

public class CreatePostRequestValidator : Validator<CreatePostRequest> {
    public CreatePostRequestValidator() {
        RuleFor(x => x.CommunityId)
            .NotEmpty().WithMessage("Community ID is required.");
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Post title is required.")
            .MaximumLength(200).WithMessage("Post title cannot exceed 200 characters.");
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Post content is required.")
            .MaximumLength(4000).WithMessage("Post content cannot exceed 4000 characters.");
    }
}

public class CreatePost(AppDbContext dbContext) : Endpoint<CreatePostRequest, CreatePostResponse> {
    public override void Configure() {
        Post("communities/{communityId}/posts");
        Version(1);
        Permissions(nameof(UserPermission.Posts_Create));
    }

    public override async Task HandleAsync(CreatePostRequest req, CancellationToken ct) {
        var communityExists = await dbContext.Communities
            .AnyAsync(c => c.Id == req.CommunityId && c.DeletedOn == null, ct);

        if (!communityExists) {
            await SendNotFoundAsync(ct);
            return;
        }

        var newPost = new Post {
            CommunityId = req.CommunityId,
            Title = req.Title,
            Content = req.Content,
            CreatedById = req.SubjectId
        };

        dbContext.Posts.Add(newPost);
        await dbContext.SaveChangesAsync(ct);

        await SendAsync(new CreatePostResponse {
            Id = newPost.Id,
            CommunityId = newPost.CommunityId,
            Title = newPost.Title,
            Content = newPost.Content,
            CreatedOn = newPost.CreatedOn
        }, 201, ct);
    }
}