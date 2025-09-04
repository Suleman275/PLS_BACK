using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using UserManagement.API.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Main.Api.Data;

namespace UserManagement.API.Endpoints.Dashboards.Users;

public class CounselorStudentCount {
    public Guid? CounselorId { get; set; }
    public string? CounselorName { get; set; }
    public int StudentCount { get; set; }
}

public class StudentsByCounselorResponse {
    public List<CounselorStudentCount> Data { get; set; } = new List<CounselorStudentCount>();
}

public class StudentsByCounselor(AppDbContext dbContext) : Endpoint<EmptyRequest, StudentsByCounselorResponse> {
    public override void Configure() {
        Get("dashboard/users/students-by-counselor");
        Version(1);
        Permissions(UserPermission.Users_Overview.ToString());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct) {
        // Step 1: Execute the translatable part of the query on the database.
        var studentGroups = await dbContext.Users
            .OfType<StudentUser>()
            .Include(s => s.Counselor)
            .GroupBy(s => s.Counselor)
            .Select(g => new {
                Counselor = g.Key,
                Count = g.Count()
            })
            .ToListAsync(ct);

        // Step 2: Perform in-memory processing on the retrieved data.
        var studentsByCounselor = studentGroups
            .Select(g => new CounselorStudentCount {
                CounselorId = g.Counselor?.Id, // Extract the ID, or null if unassigned
                CounselorName = g.Counselor != null ? $"{g.Counselor.FirstName} {g.Counselor.LastName}" : "Unassigned",
                StudentCount = g.Count
            })
            .OrderBy(c => c.CounselorName)
            .ToList();

        var res = new StudentsByCounselorResponse {
            Data = studentsByCounselor
        };

        await SendOkAsync(res, ct);
    }
}