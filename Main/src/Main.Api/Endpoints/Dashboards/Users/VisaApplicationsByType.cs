using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.Dashboards.Users;

public class VisaApplicationTypeCount {
    public Guid? VisaApplicationTypeId { get; set; }
    public string? VisaApplicationTypeName { get; set; }
    public int ApplicationCount { get; set; }
}

public record VisaApplicationsByTypeResponse {
    public List<VisaApplicationTypeCount> Data { get; set; } = new();
}

public class VisaApplicationsByType(AppDbContext dbContext) : EndpointWithoutRequest {
    public override void Configure() {
        Get("dashboard/users/visa-applications-by-type");
        Version(1);
        Permissions(UserPermission.Users_Overview.ToString());
    }

    public override async Task HandleAsync(CancellationToken ct) {
        var applicationGroups = await dbContext.VisaApplications
            .AsNoTracking()
            .Include(va => va.VisaApplicationType)
            .GroupBy(va => va.VisaApplicationType)
            .Select(g => new {
                Type = g.Key,
                Count = g.Count(),
            })
            .ToListAsync(ct);

        var applicationsByType = applicationGroups
            .Select(g => new VisaApplicationTypeCount {
                VisaApplicationTypeId = g.Type?.Id,
                VisaApplicationTypeName = g.Type != null ? g.Type.Name : "Unassigned",
                ApplicationCount = g.Count
            })
            .OrderBy(v => v.VisaApplicationTypeName)
            .ToList();

        var res = new VisaApplicationsByTypeResponse {
            Data = applicationsByType
        };

        await SendOkAsync(res, ct);
    }
}
