using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using UserManagement.API.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Main.Api.Data;

namespace UserManagement.API.Endpoints.Dashboards.Users;

public class RegistererClientCount {
    public Guid? RegistererId { get; set; }
    public string? RegistererName { get; set; }
    public int ClientCount { get; set; }
}

// Main response model for the endpoint
public class ImmigrationClientsByRegistererResponse {
    public List<RegistererClientCount> Data { get; set; } = new List<RegistererClientCount>();
}

public class ImmigrationClientsByRegisterer(AppDbContext dbContext) : Endpoint<EmptyRequest, ImmigrationClientsByRegistererResponse> {
    public override void Configure() {
        Get("dashboard/users/immigration-clients-by-registerer");
        Version(1);
        Permissions(UserPermission.Users_Overview.ToString());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct) {
        // Step 1: Execute the translatable part of the query on the database.
        var clientGroups = await dbContext.Users
            .OfType<ImmigrationClientUser>()
            .Include(ic => ic.RegisteredBy)
            .GroupBy(ic => ic.RegisteredBy)
            .Select(g => new {
                RegisteredBy = g.Key,
                Count = g.Count()
            })
            .ToListAsync(ct);

        // Step 2: Perform in-memory processing on the retrieved data.
        var clientsByRegisterer = clientGroups
            .Select(g => new RegistererClientCount {
                RegistererId = g.RegisteredBy?.Id,
                RegistererName = g.RegisteredBy != null ? $"{g.RegisteredBy.FirstName} {g.RegisteredBy.LastName}" : "Unassigned",
                ClientCount = g.Count
            })
            .OrderBy(r => r.RegistererName)
            .ToList();

        var res = new ImmigrationClientsByRegistererResponse {
            Data = clientsByRegisterer
        };

        await SendOkAsync(res, ct);
    }
}