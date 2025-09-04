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

public class UniversityApplicationStatusCount {
    public string ApplicationStatus { get; set; } = default!;
    public int ApplicationCount { get; set; }
}

public record UniversityApplicationsByStatusResponse {
    public List<UniversityApplicationStatusCount> Data { get; set; } = new();
}

public class UniversityApplicationsByStatus(AppDbContext dbContext) : EndpointWithoutRequest {
    public override void Configure() {
        Get("dashboard/users/university-applications-by-status");
        Version(1);
        Permissions(UserPermission.Users_Overview.ToString());
    }

    public override async Task HandleAsync(CancellationToken ct) {
        var statusGroups = await dbContext.UniversityApplications
            .AsNoTracking()
            .GroupBy(ua => ua.ApplicationStatus)
            .Select(g => new {
                Status = g.Key,
                Count = g.Count()
            })
            .ToListAsync(ct);

        var applicationsByStatus = statusGroups
            .Select(g => new UniversityApplicationStatusCount {
                ApplicationStatus = g.Status.ToString(),
                ApplicationCount = g.Count
            })
            .OrderBy(s => s.ApplicationStatus)
            .ToList();

        var res = new UniversityApplicationsByStatusResponse {
            Data = applicationsByStatus
        };

        await SendOkAsync(res, ct);
    }
}
