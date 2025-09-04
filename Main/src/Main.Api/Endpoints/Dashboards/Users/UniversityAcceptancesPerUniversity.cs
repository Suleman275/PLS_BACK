using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserManagement.API.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Dashboards.Users;

public class UniversityAcceptanceCount {
    public Guid? UniversityId { get; set; }
    public string UniversityName { get; set; } = default!;
    public int AcceptedApplications { get; set; }
}

public record UniversityAcceptancesResponse {
    public List<UniversityAcceptanceCount> Data { get; set; } = new();
}

public class UniversityAcceptancesPerUniversity(AppDbContext dbContext) : EndpointWithoutRequest {
    public override void Configure() {
        Get("dashboard/users/university-acceptances-per-university");
        Version(1);
        Permissions(UserPermission.Users_Overview.ToString());
    }

    public override async Task HandleAsync(CancellationToken ct) {
        var acceptanceGroups = await dbContext.UniversityApplications
            .AsNoTracking()
            .Where(ua => ua.ApplicationStatus == ApplicationStatus.Approved)
            .Include(ua => ua.UniversityProgram)
                .ThenInclude(up => up.University)
            .GroupBy(ua => ua.UniversityProgram.University)
            .Select(g => new {
                University = g.Key,
                Count = g.Count()
            })
            .ToListAsync(ct);

        var results = acceptanceGroups
            .Select(g => new UniversityAcceptanceCount {
                UniversityId = g.University?.Id,
                UniversityName = g.University != null ? g.University.Name : "Unassigned",
                AcceptedApplications = g.Count
            })
            .OrderByDescending(u => u.AcceptedApplications)
            .ToList();

        var response = new UniversityAcceptancesResponse {
            Data = results
        };

        await SendOkAsync(response, ct);
    }
}
