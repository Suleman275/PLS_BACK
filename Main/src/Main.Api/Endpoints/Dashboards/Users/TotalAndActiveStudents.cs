using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Dashboards.Users;

public class TotalAndActiveStudentsResponse {
    public int TotalStudents { get; set; }
    public int ActiveStudents { get; set; }
}

public class TotalAndActiveStudents(AppDbContext dbContext) : Endpoint<EmptyRequest, TotalAndActiveStudentsResponse> {
    public override void Configure() {
        Get("dashboard/users/total-and-active-students");
        Version(1);
        Permissions(UserPermission.Users_Overview.ToString());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct) {
        var res = new TotalAndActiveStudentsResponse {
            TotalStudents = await dbContext.Users.OfType<StudentUser>().CountAsync(ct),
            ActiveStudents = await dbContext.Users.OfType<StudentUser>().Where(s => s.IsActive).CountAsync(ct)
        };

        await SendOkAsync(res, ct);
    }
}
