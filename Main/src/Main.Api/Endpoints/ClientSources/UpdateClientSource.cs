using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.ClientSources;

public class UpdateClientSourceRequest {
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class UpdateClientSourceResponse {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public Guid CreatedById { get; set; }
    public Guid? LastModifiedById { get; set; }
}

public class UpdateClientSourceRequestValidator : Validator<UpdateClientSourceRequest> {
    public UpdateClientSourceRequestValidator() {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Client source ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Client source name is required.")
            .MaximumLength(100).WithMessage("Client source name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}

public class UpdateClientSource(AppDbContext dbContext) : Endpoint<UpdateClientSourceRequest, UpdateClientSourceResponse> {
    public override void Configure() {
        Put("client-sources/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.ClientSources_Update));
    }

    public override async Task HandleAsync(UpdateClientSourceRequest req, CancellationToken ct) {
        var clientSource = await dbContext.ClientSources
                                         .FirstOrDefaultAsync(cs => cs.Id == req.Id, ct);

        if (clientSource == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        clientSource.Name = req.Name;
        clientSource.Description = req.Description;
        clientSource.LastModifiedOn = DateTime.UtcNow;
        clientSource.LastModifiedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendOkAsync(new UpdateClientSourceResponse {
            Id = clientSource.Id,
            Name = clientSource.Name,
            Description = clientSource.Description,
            CreatedOn = clientSource.CreatedOn,
            LastModifiedOn = clientSource.LastModifiedOn,
            CreatedById = clientSource.CreatedById,
            LastModifiedById = clientSource.LastModifiedById
        }, ct);
    }
}