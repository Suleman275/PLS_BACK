using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace UserManagement.API.Endpoints.Public;

public class LocationListItemResponse {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
}

public class ListLocations(AppDbContext dbContext) : EndpointWithoutRequest<List<LocationListItemResponse>> {
    public override void Configure() {
        Get("public/locations");
        Version(1);
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct) {
        var locations = await dbContext.Locations
            .OrderBy(l => l.City)
            .ThenBy(l => l.Country)
            .Select(l => new LocationListItemResponse {
                Id = l.Id,
                Name = $"{l.City}, {l.Country}"
            })
            .ToListAsync(ct);

        await SendOkAsync(locations, ct);
    }
}