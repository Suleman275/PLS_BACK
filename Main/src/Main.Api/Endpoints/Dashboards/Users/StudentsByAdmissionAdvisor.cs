using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using UserManagement.API.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Main.Api.Data;

namespace UserManagement.API.Endpoints.Dashboards.Users;

public class AdmissionAdvisorStudentCount {
    public Guid? AdvisorId { get; set; }
    public string? AdvisorName { get; set; }
    public int StudentCount { get; set; }
}

// Main response model for the endpoint
public class StudentsByAdmissionAdvisorResponse {
    public List<AdmissionAdvisorStudentCount> Data { get; set; } = new List<AdmissionAdvisorStudentCount>();
}

public class StudentsByAdmissionAdvisor(AppDbContext dbContext) : Endpoint<EmptyRequest, StudentsByAdmissionAdvisorResponse> {
    public override void Configure() {
        Get("dashboard/users/students-by-admission-advisor");
        Version(1);
        Permissions(UserPermission.Users_Overview.ToString());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct) {
        // Step 1: Execute the translatable part of the query on the database.
        var studentGroups = await dbContext.Users
            .OfType<StudentUser>()
            .Include(s => s.AdmissionAssociate)
            .GroupBy(s => s.AdmissionAssociate)
            .Select(g => new {
                AdmissionAssociate = g.Key,
                Count = g.Count()
            })
            .ToListAsync(ct);

        // Step 2: Perform in-memory processing on the retrieved data.
        var studentsByAdvisor = studentGroups
            .Select(g => new AdmissionAdvisorStudentCount {
                AdvisorId = g.AdmissionAssociate?.Id,
                AdvisorName = g.AdmissionAssociate != null ? $"{g.AdmissionAssociate.FirstName} {g.AdmissionAssociate.LastName}" : "Unassigned",
                StudentCount = g.Count
            })
            .OrderBy(a => a.AdvisorName)
            .ToList();

        var res = new StudentsByAdmissionAdvisorResponse {
            Data = studentsByAdvisor
        };

        await SendOkAsync(res, ct);
    }
}