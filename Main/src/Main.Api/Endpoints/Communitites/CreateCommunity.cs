using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Communitites;

public class CreateCommunityRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class CreateCommunityResponse {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class CreateCommunityRequestValidator : Validator<CreateCommunityRequest> {
    public CreateCommunityRequestValidator() {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Community name is required.")
            .MaximumLength(100).WithMessage("Community name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}

public class CreateCommunity(AppDbContext dbContext) : Endpoint<CreateCommunityRequest, CreateCommunityResponse> {
    public override void Configure() {
        Post("communities");
        Version(1);
        Permissions(nameof(UserPermission.Communities_Create));
    }

    public override async Task HandleAsync(CreateCommunityRequest req, CancellationToken ct) {
        var newCommunity = new Community {
            Name = req.Name,
            Description = req.Description,
            CreatedById = req.SubjectId
        };

        dbContext.Communities.Add(newCommunity);
        await dbContext.SaveChangesAsync(ct);

        await SendAsync(new CreateCommunityResponse {
            Id = newCommunity.Id,
            Name = newCommunity.Name,
            Description = newCommunity.Description,
            CreatedOn = newCommunity.CreatedOn
        }, 201, ct);
    }
}
