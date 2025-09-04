using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using UserManagement.API.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Dashboards.Users;

public class RecentRegistration {
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public UserRole Role { get; set; }
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = default!;
    public DateTime? RegistrationDate { get; set; }
    public string? RegisteredByName { get; set; }
}

public class RecentRegistrationsResponse {
    public List<RecentRegistration> Data { get; set; } = new();
}

public class TopRecentStudentRegistrations(AppDbContext dbContext)
    : Endpoint<EmptyRequest, RecentRegistrationsResponse> {

    public override void Configure() {
        Get("dashboard/users/recent-student-registrations");
        Version(1);
        Permissions(UserPermission.Users_Overview.ToString());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct) {
        var recentStudents = await dbContext.Users
            .OfType<StudentUser>()
            .Include(u => u.RegisteredBy)
            .Where(u => u.RegistrationDate != null)
            .OrderByDescending(u => u.RegistrationDate)
            .Take(5)
            .Select(u => new RecentRegistration {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                MiddleName = u.MiddleName,
                LastName = u.LastName,
                Role = u.Role,
                RegistrationDate = u.RegistrationDate,
                RegisteredByName = u.RegisteredBy != null
                    ? $"{u.RegisteredBy.FirstName} {u.RegisteredBy.LastName}"
                    : "Unassigned"
            })
            .ToListAsync(ct);

        await SendOkAsync(new RecentRegistrationsResponse {
            Data = recentStudents
        }, ct);
    }
}
