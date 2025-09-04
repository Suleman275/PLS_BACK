namespace UserManagement.API.Endpoints.Communitites;

using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

public class UpdateCommunityRequest {
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class UpdateCommunityResponse {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public Guid CreatedById { get; set; }
    public Guid? LastModifiedById { get; set; }
}

public class UpdateCommunityRequestValidator : Validator<UpdateCommunityRequest> {
    public UpdateCommunityRequestValidator() {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Community ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Community name is required.")
            .MaximumLength(100).WithMessage("Community name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}

public class UpdateCommunity(AppDbContext dbContext) : Endpoint<UpdateCommunityRequest, UpdateCommunityResponse> {
    public override void Configure() {
        Put("communities/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.Communities_Update));
    }

    public override async Task HandleAsync(UpdateCommunityRequest req, CancellationToken ct) {
        var community = await dbContext.Communities.FirstOrDefaultAsync(c => c.Id == req.Id, ct);

        if (community == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        community.Name = req.Name;
        community.Description = req.Description;
        community.LastModifiedOn = DateTime.UtcNow;
        community.LastModifiedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendOkAsync(new UpdateCommunityResponse {
            Id = community.Id,
            Name = community.Name,
            Description = community.Description,
            CreatedOn = community.CreatedOn,
            LastModifiedOn = community.LastModifiedOn,
            CreatedById = community.CreatedById,
            LastModifiedById = community.LastModifiedById
        }, ct);
    }
}