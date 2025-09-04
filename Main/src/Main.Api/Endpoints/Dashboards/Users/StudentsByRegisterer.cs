using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Dashboards.Users;

public class RegistererStudentCount {
    public Guid? RegistererId { get; set; }
    public string? RegistererName { get; set; }
    public int StudentCount { get; set; }
}

public class StudentsByRegistererResponse {
    public List<RegistererStudentCount> Data { get; set; } = new List<RegistererStudentCount>();
}

public class StudentsByRegisterer(AppDbContext dbContext) : Endpoint<EmptyRequest, StudentsByRegistererResponse> {
    public override void Configure() {
        Get("dashboard/users/students-by-registerer");
        Version(1);
        Permissions(UserPermission.Users_Overview.ToString());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct) {
        var studentGroups = await dbContext.Users
            .OfType<StudentUser>()
            .Include(s => s.RegisteredBy)
            .GroupBy(s => s.RegisteredBy)
            .Select(g => new {
                RegisteredBy = g.Key,
                Count = g.Count()
            })
            .ToListAsync(ct);

        var studentsByRegisterer = studentGroups
            .Select(g => new RegistererStudentCount {
                RegistererId = g.RegisteredBy?.Id,
                RegistererName = g.RegisteredBy != null ? $"{g.RegisteredBy.FirstName} {g.RegisteredBy.LastName}" : "Unassigned",
                StudentCount = g.Count
            })
            .OrderBy(r => r.RegistererName)
            .ToList();

        var res = new StudentsByRegistererResponse {
            Data = studentsByRegisterer
        };

        await SendOkAsync(res, ct);
    }
}