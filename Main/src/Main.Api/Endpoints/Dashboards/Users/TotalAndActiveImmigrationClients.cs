using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Dashboards.Users;

public class TotalAndActiveImmigrationClientsResponse {
    public int TotalImmigrationClients { get; set; }
    public int ActiveImmigrationClients { get; set; }
}

public class TotalAndActiveImmigrationClients(AppDbContext dbContext) : Endpoint<EmptyRequest, TotalAndActiveImmigrationClientsResponse> {
    public override void Configure() {
        Get("dashboard/users/total-and-active-immigration-clients");
        Version(1);
        Permissions(UserPermission.Users_Overview.ToString());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct) {
        var res = new TotalAndActiveImmigrationClientsResponse {
            TotalImmigrationClients = await dbContext.Users.OfType<ImmigrationClientUser>().CountAsync(ct),
            ActiveImmigrationClients = await dbContext.Users.OfType<ImmigrationClientUser>().Where(s => s.IsActive).CountAsync(ct)
        };

        await SendOkAsync(res, ct);
    }
}