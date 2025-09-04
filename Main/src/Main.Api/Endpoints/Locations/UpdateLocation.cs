using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.Locations;

public class UpdateLocationRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }

    [FromRoute]
    public Guid Id { get; set; }
    public string City { get; set; } = default!;
    public string Country { get; set; } = default!;
}

public class UpdateLocationValidator : Validator<UpdateLocationRequest> {
    public UpdateLocationValidator() {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Location ID is required.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.")
            .MaximumLength(100).WithMessage("Country cannot exceed 100 characters.");
    }
}

public class UpdateLocationResponse {
    public string Message { get; set; } = default!;
}

public class UpdateLocation(AppDbContext dbContext) : Endpoint<UpdateLocationRequest, UpdateLocationResponse> {
    public override void Configure() {
        Put("locations/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.Locations_Update));
    }

    public override async Task HandleAsync(UpdateLocationRequest req, CancellationToken ct) {
        var locationToUpdate = await dbContext.Locations.FirstOrDefaultAsync(l => l.Id == req.Id, ct);

        if (locationToUpdate == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        if (locationToUpdate.City != req.City || locationToUpdate.Country != req.Country) {
            var existingLocationWithSameDetails = await dbContext.Locations
                .FirstOrDefaultAsync(l => l.City == req.City && l.Country == req.Country && l.Id != req.Id, ct);

            if (existingLocationWithSameDetails != null) {
                AddError(r => r.City, "Another location with this city and country already exists.");
                await SendErrorsAsync(cancellation: ct);
                return;
            }
        }

        locationToUpdate.City = req.City;
        locationToUpdate.Country = req.Country;
        locationToUpdate.LastModifiedById = req.SubjectId;
        locationToUpdate.LastModifiedOn = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        await SendAsync(new UpdateLocationResponse {
            Message = $"Location '{locationToUpdate.City}, {locationToUpdate.Country}' with ID: {locationToUpdate.Id} updated successfully."
        }, cancellation: ct);
    }
}