using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using UserManagement.API.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Main.Api.Data;

namespace UserManagement.API.Endpoints.Dashboards.Users;

public class AdmissionAdvisorClientCount {
    public Guid? AdvisorId { get; set; }
    public string? AdvisorName { get; set; }
    public int ClientCount { get; set; }
}

// Main response model for the endpoint
public class ImmigrationClientsByAdmissionAdvisorResponse {
    public List<AdmissionAdvisorClientCount> Data { get; set; } = new List<AdmissionAdvisorClientCount>();
}

public class ImmigrationClientsByAdmissionAdvisor(AppDbContext dbContext) : Endpoint<EmptyRequest, ImmigrationClientsByAdmissionAdvisorResponse> {
    public override void Configure() {
        Get("dashboard/users/immigration-clients-by-admission-advisor");
        Version(1);
        Permissions(UserPermission.Users_Overview.ToString());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct) {
        // Step 1: Execute the translatable part of the query on the database.
        var clientGroups = await dbContext.Users
            .OfType<ImmigrationClientUser>()
            .Include(ic => ic.AdmissionAssociate)
            .GroupBy(ic => ic.AdmissionAssociate)
            .Select(g => new {
                AdmissionAssociate = g.Key,
                Count = g.Count()
            })
            .ToListAsync(ct);

        // Step 2: Perform in-memory processing on the retrieved data.
        var clientsByAdvisor = clientGroups
            .Select(g => new AdmissionAdvisorClientCount {
                AdvisorId = g.AdmissionAssociate?.Id,
                AdvisorName = g.AdmissionAssociate != null ? $"{g.AdmissionAssociate.FirstName} {g.AdmissionAssociate.LastName}" : "Unassigned",
                ClientCount = g.Count
            })
            .OrderBy(a => a.AdvisorName)
            .ToList();

        var res = new ImmigrationClientsByAdmissionAdvisorResponse {
            Data = clientsByAdvisor
        };

        await SendOkAsync(res, ct);
    }
}