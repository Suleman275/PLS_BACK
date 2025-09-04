using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using UserManagement.API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Main.Api.Data;

namespace UserManagement.API.Endpoints.Dashboards.Users;

public class NationalityStudentCount {
    public Guid? NationalityId { get; set; }
    public string? NationalityName { get; set; }
    public int Count { get; set; }
}

// Response model for the endpoint, containing a list of NationalityStudentCount
public class StudentsByNationalityResponse {
    public List<NationalityStudentCount> Data { get; set; } = new List<NationalityStudentCount>();
}

public class StudentsByNationality(AppDbContext dbContext) : Endpoint<EmptyRequest, StudentsByNationalityResponse> {
    public override void Configure() {
        Get("dashboard/users/students-by-nationality");
        Version(1);
        Permissions(UserPermission.Users_Overview.ToString());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct) {
        // Step 1: Execute the translatable part of the query on the database.
        var studentsByNationalityGroups = await dbContext.Users
            .OfType<StudentUser>()
            .Include(s => s.Nationality)
            .GroupBy(s => s.Nationality)
            .Select(g => new {
                Nationality = g.Key,
                Count = g.Count()
            })
            .ToListAsync(ct);

        // Step 2: Perform in-memory processing on the retrieved data.
        var studentsByNationality = studentsByNationalityGroups
            .Select(g => new NationalityStudentCount {
                NationalityId = g.Nationality?.Id,
                NationalityName = g.Nationality != null ? g.Nationality.Name : "Unassigned",
                Count = g.Count
            })
            .OrderBy(n => n.NationalityName)
            .ToList();

        var res = new StudentsByNationalityResponse {
            Data = studentsByNationality
        };

        await SendOkAsync(res, ct);
    }
}