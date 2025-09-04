using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Locations;

public class CreateLocationRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    public string City { get; set; } = default!;
    public string Country { get; set; } = default!;
}

public class CreateLocationResponse {
    public string Message { get; set; } = default!;
}

public class CreateLocationValidator : Validator<CreateLocationRequest> {
    public CreateLocationValidator() {
        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.")
            .MaximumLength(100).WithMessage("Country cannot exceed 100 characters.");
    }
}

public class CreateLocation(AppDbContext dbContext) : Endpoint<CreateLocationRequest, CreateLocationResponse> {
    public override void Configure() {
        Post("locations");
        Version(1);
        Permissions(nameof(UserPermission.Locations_Create));
    }

    public override async Task HandleAsync(CreateLocationRequest req, CancellationToken ct) {
        var existingLocation = await dbContext.Locations
            .FirstOrDefaultAsync(l => l.City == req.City && l.Country == req.Country, ct);

        if (existingLocation != null) {
            AddError(r => r.City, "A location with this city and country already exists.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var newLocation = new Location {
            City = req.City,
            Country = req.Country,
            CreatedById = req.SubjectId,
        };

        dbContext.Locations.Add(newLocation);
        await dbContext.SaveChangesAsync(ct);

        await SendAsync(new CreateLocationResponse {
            Message = $"Location '{newLocation.City}, {newLocation.Country}' created successfully with ID: {newLocation.Id}."
        }, cancellation: ct);
    }
}