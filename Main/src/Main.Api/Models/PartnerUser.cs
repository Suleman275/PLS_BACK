using UserManagement.API.Enums;

namespace UserManagement.API.Models;

public class PartnerUser : User {
    public override UserRole Role { get; set; } = UserRole.Partner;
}
