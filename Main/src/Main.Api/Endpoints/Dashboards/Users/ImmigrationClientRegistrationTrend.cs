using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using UserManagement.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Main.Api.Data;

namespace UserManagement.API.Endpoints.Dashboards.Users;

public class ImmigrationClientRegistrationTrendRequest {
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class ImmigrationClientRegistrationTrendItem {
    public string Period { get; set; } = default!;
    public int ClientCount { get; set; }
}

public class ImmigrationClientRegistrationTrendResponse {
    public List<ImmigrationClientRegistrationTrendItem> Trend { get; set; } = new List<ImmigrationClientRegistrationTrendItem>();
}

public class ImmigrationClientRegistrationTrend(AppDbContext dbContext) : Endpoint<ImmigrationClientRegistrationTrendRequest, ImmigrationClientRegistrationTrendResponse> {
    public override void Configure() {
        Post("dashboard/users/immigration-client-registration-trend");
        Version(1);
        Permissions(UserPermission.Users_Overview.ToString());
    }

    public override async Task HandleAsync(ImmigrationClientRegistrationTrendRequest req, CancellationToken ct) {
        if (req.StartDate > req.EndDate) {
            AddError("StartDate cannot be after EndDate.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        // *** FIX: Convert incoming dates to UTC ***
        var utcStartDate = DateTime.SpecifyKind(req.StartDate, DateTimeKind.Utc);
        var utcEndDate = DateTime.SpecifyKind(req.EndDate, DateTimeKind.Utc);

        // Step 1: Execute a translatable query to get raw data
        var registrationTrendData = await dbContext.Users
            .OfType<ImmigrationClientUser>()
            .Where(ic => ic.RegistrationDate.HasValue &&
                        ic.RegistrationDate.Value >= utcStartDate &&
                        ic.RegistrationDate.Value <= utcEndDate)
            .GroupBy(ic => new {
                Year = ic.RegistrationDate!.Value.Year,
                Month = ic.RegistrationDate.Value.Month
            })
            .Select(g => new {
                Year = g.Key.Year,
                Month = g.Key.Month,
                ClientCount = g.Count()
            })
            .ToListAsync(ct);

        // Step 2: Perform string formatting and ordering on the in-memory data
        var res = new ImmigrationClientRegistrationTrendResponse {
            Trend = registrationTrendData
                .Select(item => new ImmigrationClientRegistrationTrendItem {
                    Period = $"{item.Year}-{item.Month:00}",
                    ClientCount = item.ClientCount
                })
                .OrderBy(item => item.Period)
                .ToList()
        };

        await SendOkAsync(res, ct);
    }
}