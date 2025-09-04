using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Dashboards.Users;

public class LocationClientCount {
    public Guid? LocationId { get; set; }
    public string? LocationName { get; set; }
    public int ClientCount { get; set; }
}

public class ImmigrationClientsByLocationResponse {
    public List<LocationClientCount> Data { get; set; } = new List<LocationClientCount>();
}

public class ImmigrationClientsByLocation(AppDbContext dbContext) : Endpoint<EmptyRequest, ImmigrationClientsByLocationResponse> {
    public override void Configure() {
        Get("dashboard/users/immigration-clients-by-location");
        Version(1);
        Permissions(UserPermission.Users_Overview.ToString());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct) {
        var clientsByLocationGroups = await dbContext.Users
            .OfType<ImmigrationClientUser>()
            .Include(ic => ic.Location)
            .GroupBy(ic => ic.Location)
            .Select(g => new {
                Location = g.Key,
                Count = g.Count()
            })
            .ToListAsync(ct);

        var immigrationClientsByLocation = clientsByLocationGroups
            .Select(g => new LocationClientCount {
                LocationId = g.Location?.Id,
                LocationName = g.Location != null ? $"{g.Location.City}, {g.Location.Country}" : "Unassigned",
                ClientCount = g.Count
            })
            .OrderBy(l => l.LocationName)
            .ToList();

        var res = new ImmigrationClientsByLocationResponse {
            Data = immigrationClientsByLocation
        };

        await SendOkAsync(res, ct);
    }
}