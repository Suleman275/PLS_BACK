using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using UserManagement.API.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Main.Api.Data;

namespace UserManagement.API.Endpoints.Dashboards.Users;

public class CounselorClientCount {
    public Guid? CounselorId { get; set; }
    public string? CounselorName { get; set; }
    public int ClientCount { get; set; }
}

// Main response model for the endpoint
public class ImmigrationClientsByCounselorResponse {
    public List<CounselorClientCount> Data { get; set; } = new List<CounselorClientCount>();
}

public class ImmigrationClientsByCounselor(AppDbContext dbContext) : Endpoint<EmptyRequest, ImmigrationClientsByCounselorResponse> {
    public override void Configure() {
        Get("dashboard/users/immigration-clients-by-counselor");
        Version(1);
        Permissions(UserPermission.Users_Overview.ToString());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct) {
        // Step 1: Execute the translatable part of the query on the database.
        var clientGroups = await dbContext.Users
            .OfType<ImmigrationClientUser>()
            .Include(ic => ic.Counselor)
            .GroupBy(ic => ic.Counselor)
            .Select(g => new {
                Counselor = g.Key,
                Count = g.Count()
            })
            .ToListAsync(ct);

        // Step 2: Perform in-memory processing on the retrieved data.
        var clientsByCounselor = clientGroups
            .Select(g => new CounselorClientCount {
                CounselorId = g.Counselor?.Id, // Extract the ID, or null if unassigned
                CounselorName = g.Counselor != null ? $"{g.Counselor.FirstName} {g.Counselor.LastName}" : "Unassigned",
                ClientCount = g.Count
            })
            .OrderBy(c => c.CounselorName)
            .ToList();

        var res = new ImmigrationClientsByCounselorResponse {
            Data = clientsByCounselor
        };

        await SendOkAsync(res, ct);
    }
}