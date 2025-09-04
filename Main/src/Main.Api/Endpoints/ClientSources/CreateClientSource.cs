using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.ClientSources;

public class CreateClientSourceRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class CreateClientSourceResponse {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class CreateClientSourceRequestValidator : Validator<CreateClientSourceRequest> {
    public CreateClientSourceRequestValidator() {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Client source name is required.")
            .MaximumLength(100).WithMessage("Client source name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}

public class CreateClientSource(AppDbContext dbContext) : Endpoint<CreateClientSourceRequest, CreateClientSourceResponse> {
    public override void Configure() {
        Post("client-sources");
        Version(1);
        Permissions(nameof(UserPermission.ClientSources_Create));
    }

    public override async Task HandleAsync(CreateClientSourceRequest req, CancellationToken ct) {
        var newClientSource = new ClientSource {
            Name = req.Name,
            Description = req.Description,
            CreatedById = req.SubjectId,
        };

        dbContext.ClientSources.Add(newClientSource);
        await dbContext.SaveChangesAsync(ct);

        await SendAsync(new CreateClientSourceResponse {
            Id = newClientSource.Id,
            Name = newClientSource.Name,
            Description = newClientSource.Description,
            CreatedOn = newClientSource.CreatedOn
        }, 201, ct);
    }
}