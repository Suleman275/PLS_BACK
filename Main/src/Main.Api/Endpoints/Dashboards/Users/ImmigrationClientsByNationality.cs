using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using UserManagement.API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Main.Api.Data;

namespace UserManagement.API.Endpoints.Dashboards.Users;
// A new, separate DTO for clients to ensure clarity
public class NationalityClientCount {
    public Guid? NationalityId { get; set; }
    public string? NationalityName { get; set; }
    public int ClientCount { get; set; }
}


public class ImmigrationClientsByNationalityResponse {
    public List<NationalityClientCount> Data { get; set; } = new List<NationalityClientCount>();
}

public class ImmigrationClientsByNationality(AppDbContext dbContext) : Endpoint<EmptyRequest, ImmigrationClientsByNationalityResponse> {
    public override void Configure() {
        Get("dashboard/users/immigration-clients-by-nationality");
        Version(1);
        Permissions(UserPermission.Users_Overview.ToString());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct) {
        // Step 1: Execute the translatable part of the query on the database.
        var clientsByNationalityGroups = await dbContext.Users
            .OfType<ImmigrationClientUser>()
            .Include(ic => ic.Nationality)
            .GroupBy(ic => ic.Nationality)
            .Select(g => new {
                Nationality = g.Key,
                Count = g.Count()
            })
            .ToListAsync(ct);

        // Step 2: Perform in-memory processing on the retrieved data.
        var immigrationClientsByNationality = clientsByNationalityGroups
            .Select(g => new NationalityClientCount {
                NationalityId = g.Nationality?.Id,
                NationalityName = g.Nationality != null ? g.Nationality.Name : "Unassigned",
                ClientCount = g.Count
            })
            .OrderBy(n => n.NationalityName)
            .ToList();

        var res = new ImmigrationClientsByNationalityResponse {
            Data = immigrationClientsByNationality
        };

        await SendOkAsync(res, ct);
    }
}