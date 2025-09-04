using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Dashboards.Users;

public class ClientSourceClientCount {
    public Guid? ClientSourceId { get; set; }
    public string? ClientSourceName { get; set; }
    public int ClientCount { get; set; }
}

public class ImmigrationClientsByClientSourceResponse {
    public List<ClientSourceClientCount> Data { get; set; } = new List<ClientSourceClientCount>();
}

public class ImmigrationClientsByClientSource(AppDbContext dbContext) : Endpoint<EmptyRequest, ImmigrationClientsByClientSourceResponse> {
    public override void Configure() {
        Get("dashboard/users/immigration-clients-by-client-source");
        Version(1);
        Permissions(UserPermission.Users_Overview.ToString());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct) {
        var clientsByClientSourceGroups = await dbContext.Users
            .OfType<ImmigrationClientUser>()
            .Include(ic => ic.ClientSource)
            .GroupBy(ic => ic.ClientSource)
            .Select(g => new {
                ClientSource = g.Key,
                Count = g.Count()
            })
            .ToListAsync(ct);

        var immigrationClientsByClientSource = clientsByClientSourceGroups
            .Select(g => new ClientSourceClientCount {
                ClientSourceId = g.ClientSource?.Id,
                ClientSourceName = g.ClientSource != null ? g.ClientSource.Name : "Unassigned",
                ClientCount = g.Count
            })
            .OrderBy(l => l.ClientSourceName)
            .ToList();

        var res = new ImmigrationClientsByClientSourceResponse {
            Data = immigrationClientsByClientSource
        };

        await SendOkAsync(res, ct);
    }
}
