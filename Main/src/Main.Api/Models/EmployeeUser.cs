using SharedKernel.Enums;
using UserManagement.API.Constants;
using UserManagement.API.Enums;

namespace UserManagement.API.Models;

public class EmployeeUser : User {
    public override UserRole Role { get; set; } = UserRole.Employee;

    public string? Title { get; set; }
}