using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.EmployeeUsers;

public class ListAllEmployeeUsersRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
}

public class ListAllEmployeeUsersResponse {
    public IEnumerable<EmployeeUserListItem> Employees { get; set; } = Enumerable.Empty<EmployeeUserListItem>();
    public int TotalCount { get; set; }
}


public class ListAllEmployeeUsers(AppDbContext dbContext) : Endpoint<ListAllEmployeeUsersRequest, ListAllEmployeeUsersResponse> {
    public override void Configure() {
        Get("employees/all");
        Version(1);
        Permissions(nameof(UserPermission.Employees_Read));
    }

    public override async Task HandleAsync(ListAllEmployeeUsersRequest req, CancellationToken ct) {
        var query = dbContext.Users.OfType<EmployeeUser>().AsQueryable();

        var totalCount = await query.CountAsync(ct);

        query = query.OrderBy(u => u.LastName).ThenBy(u => u.FirstName);

        var employeeUsers = await query
                .Select(u => new EmployeeUserListItem {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    MiddleName = u.MiddleName,
                    LastName = u.LastName,
                    Title = u.Title,
                    PhoneNumber = u.PhoneNumber,
                    DateOfBirth = u.DateOfBirth,
                    Permissions = u.Permissions.Select(p => p.Permission),
                    IsActive = u.IsActive,
                    IsEmailVerified = u.IsEmailVerified,
                    IsPhoneVerified = u.IsPhoneVerified,
                })
                .ToListAsync(ct);


        await SendOkAsync(new ListAllEmployeeUsersResponse {
            Employees = employeeUsers,
            TotalCount = totalCount,
        }, ct);
    }
}