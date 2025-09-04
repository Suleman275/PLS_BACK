using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using System.Threading;
using System.Threading.Tasks;
using UserManagement.API.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Dashboards.Users;

public class VisaApplicationSuccessRateResponse {
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    //public int TotalDecided => ApprovedCount + RejectedCount;
    /// <summary>
    /// Success rate = Approved / (Approved + Rejected). Null if no decisions yet.
    /// </summary>
    public double? SuccessRate { get; set; }
}

public class VisaApplicationSuccessRate(AppDbContext dbContext) : EndpointWithoutRequest {
    public override void Configure() {
        Get("dashboard/users/visa-application-success-rate");
        Version(1);
        Permissions(UserPermission.Users_Overview.ToString());
    }

    public override async Task HandleAsync(CancellationToken ct) {
        var decisionCounts = await dbContext.VisaApplications
            .AsNoTracking()
            .Where(va => va.ApplicationStatus == ApplicationStatus.Approved
                      || va.ApplicationStatus == ApplicationStatus.Rejected)
            .GroupBy(va => va.ApplicationStatus)
            .Select(g => new {
                Status = g.Key,
                Count = g.Count()
            })
            .ToListAsync(ct);

        var approved = decisionCounts.FirstOrDefault(g => g.Status == ApplicationStatus.Approved)?.Count ?? 0;
        var rejected = decisionCounts.FirstOrDefault(g => g.Status == ApplicationStatus.Rejected)?.Count ?? 0;
        var total = approved + rejected;

        double? successRate = total > 0
            ? (double)approved / total
            : null;

        var response = new VisaApplicationSuccessRateResponse {
            ApprovedCount = approved,
            RejectedCount = rejected,
            SuccessRate = successRate
        };

        await SendOkAsync(response, ct);
    }
}
