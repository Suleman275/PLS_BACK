//// Endpoints/Employee/GetEmployeeById.cs
//using FastEndpoints;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using SharedKernel.Enums; // For UserPermissions
//using UserManagement.API.Data;
//using UserManagement.API.Enums;
//using UserManagement.API.Models;

//namespace UserManagement.API.Endpoints.Employees;

//public class GetEmployeeByIdRequest {
//    [FromRoute]
//    public Guid Id { get; set; }
//}

//public class EmployeeDetailsResponse {
//    public Guid Id { get; set; }
//    public string Email { get; set; } = default!;
//    public string FirstName { get; set; } = default!;
//    public string? MiddleName { get; set; }
//    public string LastName { get; set; } = default!;
//    public UserRole Role { get; set; }
//    public UserPermissions Permissions { get; set; }
//    public bool IsActive { get; set; }
//    public bool IsEmailVerified { get; set; }
//    public bool IsPhoneVerified { get; set; }
//    public string? PhoneNumber { get; set; }
//    public DateTime? DateOfBirth { get; set; }
//    public string? ProfilePictureUrl { get; set; }
//    public string? Title { get; set; }
//    public DateTime CreatedOn { get; set; }
//    public Guid CreatedById { get; set; }
//    public DateTime? LastModifiedOn { get; set; }
//    public Guid? LastModifiedById { get; set; }
//}

//public class GetEmployeeById(AppDbContext dbContext) : Endpoint<GetEmployeeByIdRequest, EmployeeDetailsResponse> {
//    public override void Configure() {
//        Get("employees/{Id}");
//        Version(1);
//        Permissions(UserPermissions.Employees_View.ToString()); // Only users with Employees_View can access this
//        Summary(s => s.Summary = "Get detailed information about a specific employee by ID.");
//    }

//    public override async Task HandleAsync(GetEmployeeByIdRequest req, CancellationToken ct) {
//        var employee = await dbContext.Users
//            .OfType<EmployeeUser>() // Ensure we only fetch EmployeeUser
//            .Where(e => e.Id == req.Id)
//            .FirstOrDefaultAsync(ct);


//        if (employee is null) {
//            await SendNotFoundAsync(ct);
//            return;
//        }

//        var e = employee;

//        var res = new EmployeeDetailsResponse {
//            Id = e.Id,
//            Email = e.Email,
//            FirstName = e.FirstName,
//            MiddleName = e.MiddleName,
//            LastName = e.LastName,
//            Role = e.Role,
//            Permissions = e.Permissions,
//            IsActive = e.IsActive,
//            IsEmailVerified = e.IsEmailVerified,
//            IsPhoneVerified = e.IsPhoneVerified,
//            PhoneNumber = e.PhoneNumber,
//            DateOfBirth = e.DateOfBirth,
//            ProfilePictureUrl = e.ProfilePictureUrl,
//            Title = e.Title,
//            CreatedOn = e.CreatedOn,
//            CreatedById = e.CreatedById,
//            LastModifiedOn = e.LastModifiedOn,
//            LastModifiedById = e.LastModifiedById
//        };

//        await SendOkAsync(res, ct);
//    }
//}