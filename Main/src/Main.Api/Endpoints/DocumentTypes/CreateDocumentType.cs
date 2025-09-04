using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.DocumentTypes;

public class CreateDocumentTypeRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class CreateDocumentTypeResponse {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class CreateDocumentTypeValidator : Validator<CreateDocumentTypeRequest> {
    public CreateDocumentTypeValidator() {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");
    }
}

public class CreateDocumentType(AppDbContext dbContext) : Endpoint<CreateDocumentTypeRequest, CreateDocumentTypeResponse> {
    public override void Configure() {
        Post("document-types");
        Version(1);
        Permissions(nameof(UserPermission.DocumentTypes_Create));
    }

    public override async Task HandleAsync(CreateDocumentTypeRequest req, CancellationToken ct) {
        var documentType = new DocumentType {
            Name = req.Name,
            Description = req.Description,
            CreatedById = req.SubjectId,
        };

        dbContext.DocumentTypes.Add(documentType);
        await dbContext.SaveChangesAsync(ct);

        await SendCreatedAtAsync("GetDocumentTypeById", new { documentType.Id }, new CreateDocumentTypeResponse {
            Id = documentType.Id,
            Name = documentType.Name,
            Description = documentType.Description
        }, cancellation: ct);
    }
}