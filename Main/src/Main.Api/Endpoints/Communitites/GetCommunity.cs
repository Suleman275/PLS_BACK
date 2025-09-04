using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.Communitites;

public class GetCommunityByIdRequest {
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
}

public class GetCommunityByIdResponse {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public Guid CreatedById { get; set; }
    public Guid? LastModifiedById { get; set; }
}

public class GetCommunityByIdRequestValidator : Validator<GetCommunityByIdRequest> {
    public GetCommunityByIdRequestValidator() {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Community ID is required.");
    }
}

public class GetCommunityById(AppDbContext dbContext) : Endpoint<GetCommunityByIdRequest, GetCommunityByIdResponse> {
    public override void Configure() {
        Get("communities/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.Communities_Read));
    }

    public override async Task HandleAsync(GetCommunityByIdRequest req, CancellationToken ct) {
        var community = await dbContext.Communities
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == req.Id && c.DeletedOn == null, ct);

        if (community == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendOkAsync(new GetCommunityByIdResponse {
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