using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Dashboards.Users;

public class LocationStudentCount {
    public Guid? LocationId { get; set; }
    public string? LocationName { get; set; }
    public int Count { get; set; }
}

public class StudentsByLocationResponse {
    public List<LocationStudentCount> Data { get; set; } = new List<LocationStudentCount>();
}

public class StudentsByLocation(AppDbContext dbContext) : Endpoint<EmptyRequest, StudentsByLocationResponse> {
    public override void Configure() {
        Get("dashboard/users/students-by-location");
        Version(1);
        Permissions(UserPermission.Users_Overview.ToString());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct) {
        var studentsByLocationGroups = await dbContext.Users
            .OfType<StudentUser>()
            .Include(s => s.Location)
            .GroupBy(s => s.Location)
            .Select(g => new {
                Location = g.Key,
                Count = g.Count()
            })
            .ToListAsync(ct);

        var studentsByLocation = studentsByLocationGroups
            .Select(g => new LocationStudentCount {
                LocationId = g.Location?.Id,
                LocationName = g.Location != null ? $"{g.Location.City}, {g.Location.Country}" : "Unassigned",
                Count = g.Count
            })
            .OrderBy(l => l.LocationName)
            .ToList();

        var res = new StudentsByLocationResponse {
            Data = studentsByLocation
        };

        await SendOkAsync(res, ct);
    }
}