using FastEndpoints;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.EmployeeUsers;

public class DeleteEmployeeRequest {
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
}

public class DeleteEmployee(AppDbContext dbContext) : Endpoint<DeleteEmployeeRequest, EmptyResponse> {
    public override void Configure() {
        Delete("employees/{id}");
        Version(1);
        Permissions(nameof(UserPermission.Employees_Delete));
    }

    public override async Task HandleAsync(DeleteEmployeeRequest req, CancellationToken ct) {
        var employee = await dbContext.Users.OfType<EmployeeUser>().FirstOrDefaultAsync(u => u.Id == req.Id, ct);

        if (employee is null) {
            await SendNotFoundAsync(ct);
            return;
        }

        employee.DeletedOn = DateTime.UtcNow;
        employee.DeletedById = req.SubjectId;
        employee.IsActive = false;

        await dbContext.SaveChangesAsync(ct);

        await SendNoContentAsync(ct);
    }
}