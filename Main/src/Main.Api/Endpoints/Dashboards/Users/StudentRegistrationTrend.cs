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

public class StudentRegistrationTrendRequest {
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class StudentRegistrationTrendItem {
    public string Period { get; set; } = default!;
    public int StudentCount { get; set; }
}

public class StudentRegistrationTrendResponse {
    public List<StudentRegistrationTrendItem> Trend { get; set; } = new List<StudentRegistrationTrendItem>();
}

public class StudentRegistrationTrend(AppDbContext dbContext) : Endpoint<StudentRegistrationTrendRequest, StudentRegistrationTrendResponse> {
    public override void Configure() {
        Post("dashboard/users/student-registration-trend");
        Version(1);
        Permissions(UserPermission.Users_Overview.ToString());
    }

    public override async Task HandleAsync(StudentRegistrationTrendRequest req, CancellationToken ct) {
        if (req.StartDate > req.EndDate) {
            AddError("StartDate cannot be after EndDate.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var utcStartDate = DateTime.SpecifyKind(req.StartDate, DateTimeKind.Utc);
        var utcEndDate = DateTime.SpecifyKind(req.EndDate, DateTimeKind.Utc);

        var registrationTrendData = await dbContext.Users
            .OfType<StudentUser>()
            .Where(s => s.RegistrationDate.HasValue &&
                        s.RegistrationDate.Value >= utcStartDate && // Use the new UTC variables
                        s.RegistrationDate.Value <= utcEndDate)     // Use the new UTC variables
            .GroupBy(s => new {
                Year = s.RegistrationDate!.Value.Year,
                Month = s.RegistrationDate.Value.Month
            })
            .Select(g => new {
                Year = g.Key.Year,
                Month = g.Key.Month,
                StudentCount = g.Count()
            })
            .ToListAsync(ct);

        var res = new StudentRegistrationTrendResponse {
            Trend = registrationTrendData
                .Select(item => new StudentRegistrationTrendItem {
                    Period = $"{item.Year}-{item.Month:00}",
                    StudentCount = item.StudentCount
                })
                .OrderBy(item => item.Period)
                .ToList()
        };

        await SendOkAsync(res, ct);
    }
}