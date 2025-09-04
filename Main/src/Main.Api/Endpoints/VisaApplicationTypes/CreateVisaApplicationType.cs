using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Configurations;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.VisaApplicationTypes;

public class CreateVisaApplicationTypeRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class CreateVisaApplicationTypeResponse {
    public Guid Id { get; set; }
    public string Message { get; set; } = "Visa Application Type Created Successfully";
}

public class CreateVisaApplicationTypeRequestValidator : AbstractValidator<CreateVisaApplicationTypeRequest> {
    public CreateVisaApplicationTypeRequestValidator() {
        RuleFor(vat => vat.Name).NotEmpty();
    }
}

public class CreateVisaApplicationType(AppDbContext dbContext) : Endpoint<CreateVisaApplicationTypeRequest, CreateVisaApplicationTypeResponse> {
    public override void Configure() {
        Post("visa-application-types");
        Version(1);
        Permissions(nameof(UserPermission.VisaApplicationTypes_Create));
    }

    public override async Task HandleAsync(CreateVisaApplicationTypeRequest req, CancellationToken ct) {
        var vat = new VisaApplicationType {
            Name = req.Name,
            Description = req.Description,
            CreatedById = req.SubjectId,
        };

        dbContext.VisaApplicationTypes.Add(vat);
        await dbContext.SaveChangesAsync();

        var res = new CreateVisaApplicationTypeResponse {
            Id = vat.Id
        };

        await SendOkAsync(res, ct);
    }
}