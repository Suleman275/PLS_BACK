using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Dashboards.Users;

// DTO for a single client source and its student count
public class ClientSourceStudentCount {
    public Guid? ClientSourceId { get; set; }
    public string? ClientSourceName { get; set; }
    public int Count { get; set; }
}

// Response DTO for the endpoint
public class StudentsByClientSourceResponse {
    public List<ClientSourceStudentCount> Data { get; set; } = new List<ClientSourceStudentCount>();
}

// Endpoint to get the count of students grouped by their client source.
public class StudentsByClientSource(AppDbContext dbContext) : Endpoint<EmptyRequest, StudentsByClientSourceResponse> {
    public override void Configure() {
        Get("dashboard/users/students-by-client-source");
        Version(1);
        Permissions(UserPermission.Users_Overview.ToString());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct) {
        // Query the database to find all students, group them by their client source,
        // and get a count for each group.
        var studentsByClientSourceGroups = await dbContext.Users
            .OfType<StudentUser>()
            .Include(s => s.ClientSource)
            .GroupBy(s => s.ClientSource)
            .Select(g => new {
                ClientSource = g.Key,
                Count = g.Count()
            })
            .ToListAsync(ct);

        // Map the grouped data to the response DTO.
        // If a client source is unassigned (null), label it accordingly.
        var studentsByClientSource = studentsByClientSourceGroups
            .Select(g => new ClientSourceStudentCount {
                ClientSourceId = g.ClientSource?.Id,
                ClientSourceName = g.ClientSource != null ? g.ClientSource.Name : "Unassigned",
                Count = g.Count
            })
            .OrderBy(l => l.ClientSourceName)
            .ToList();

        var res = new StudentsByClientSourceResponse {
            Data = studentsByClientSource
        };

        await SendOkAsync(res, ct);
    }
}
