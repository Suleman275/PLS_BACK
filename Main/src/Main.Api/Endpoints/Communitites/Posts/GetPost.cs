using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.Communitites.Posts;

public class GetPostByIdRequest {
    [FromRoute]
    public Guid CommunityId { get; set; }
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
}

public class GetPostByIdResponse {
    public Guid Id { get; set; }
    public Guid CommunityId { get; set; }
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public Guid CreatedById { get; set; }
    public Guid? LastModifiedById { get; set; }
}

public class GetPostByIdRequestValidator : Validator<GetPostByIdRequest> {
    public GetPostByIdRequestValidator() {
        RuleFor(x => x.CommunityId)
            .NotEmpty().WithMessage("Community ID is required.");
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Post ID is required.");
    }
}

public class GetPostById(AppDbContext dbContext) : Endpoint<GetPostByIdRequest, GetPostByIdResponse> {
    public override void Configure() {
        Get("communities/{communityId}/posts/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.Posts_Read));
    }

    public override async Task HandleAsync(GetPostByIdRequest req, CancellationToken ct) {
        var post = await dbContext.Posts
            .AsNoTracking()
            .FirstOrDefaultAsync(p =>
                p.CommunityId == req.CommunityId &&
                p.Id == req.Id &&
                p.DeletedOn == null, ct);

        if (post == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendOkAsync(new GetPostByIdResponse {
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