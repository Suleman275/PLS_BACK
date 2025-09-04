using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.Dashboards.Users;

public class VisaApplicationStatusCount {
    public string ApplicationStatus { get; set; } = default!;
    public int ApplicationCount { get; set; }
}

public record VisaApplicationsByStatusResponse {
    public List<VisaApplicationStatusCount> Data { get; set; } = new();
}

public class VisaApplicationsByStatus(AppDbContext dbContext) : EndpointWithoutRequest {
    public override void Configure() {
        Get("dashboard/users/visa-applications-by-status");
        Version(1);
        Permissions(UserPermission.Users_Overview.ToString());
    }

    public override async Task HandleAsync(CancellationToken ct) {
        var statusGroups = await dbContext.VisaApplications
            .AsNoTracking()
            .GroupBy(va => va.ApplicationStatus)
            .Select(g => new {
                Status = g.Key,
                Count = g.Count()
            })
            .ToListAsync(ct);

        var applicationsByStatus = statusGroups
            .Select(g => new VisaApplicationStatusCount {
                ApplicationStatus = g.Status.ToString(),
                ApplicationCount = g.Count
            })
            .OrderBy(s => s.ApplicationStatus)
            .ToList();

        var res = new VisaApplicationsByStatusResponse {
            Data = applicationsByStatus
        };

        await SendOkAsync(res, ct);
    }
}
