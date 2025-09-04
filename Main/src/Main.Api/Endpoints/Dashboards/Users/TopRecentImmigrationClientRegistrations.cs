using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using UserManagement.API.Endpoints.Dashboards.Users;
using UserManagement.API.Models;

public class TopRecentImmigrationClientRegistrations(AppDbContext dbContext)
    : Endpoint<EmptyRequest, RecentRegistrationsResponse> {

    public override void Configure() {
        Get("dashboard/users/recent-immigration-client-registrations");
        Version(1);
        Permissions(UserPermission.Users_Overview.ToString());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct) {
        var recentClients = await dbContext.Users
            .OfType<ImmigrationClientUser>()
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
            Data = recentClients
        }, ct);
    }
}
