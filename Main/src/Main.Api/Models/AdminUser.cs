using SharedKernel.Enums;
using UserManagement.API.Constants;
using UserManagement.API.Enums;

namespace UserManagement.API.Models;

public class AdminUser : User {
    public override UserRole Role { get; set; } = UserRole.Admin;
}