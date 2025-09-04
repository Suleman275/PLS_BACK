using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.DocumentTypes;

public class UpdateDocumentTypeRequest {
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class UpdateDocumentTypeResponse {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class UpdateDocumentTypeValidator : Validator<UpdateDocumentTypeRequest> {
    public UpdateDocumentTypeValidator() {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID is required for update.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");
    }
}

public class UpdateDocumentType(AppDbContext dbContext) : Endpoint<UpdateDocumentTypeRequest, UpdateDocumentTypeResponse> {
    public override void Configure() {
        Put("document-types/{id}");
        Version(1);
        Permissions(nameof(UserPermission.DocumentTypes_Update));
    }

    public override async Task HandleAsync(UpdateDocumentTypeRequest req, CancellationToken ct) {
        var documentType = await dbContext.DocumentTypes.FirstOrDefaultAsync(dt => dt.Id == req.Id, ct);

        if (documentType is null) {
            await SendNotFoundAsync(ct);
            return;
        }

        documentType.Name = req.Name;
        documentType.Description = req.Description;
        documentType.LastModifiedOn = DateTime.UtcNow;
        documentType.LastModifiedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendAsync(new UpdateDocumentTypeResponse {
            Id = documentType.Id,
            Name = documentType.Name,
            Description = documentType.Description
        }, cancellation: ct);
    }
}