using SharedKernel.Enums;
using SharedKernel.Models;

namespace UserManagement.API.Models;

public class PermissionAssignment : EntityBase {
    public Guid UserId { get; set; }
    public User User { get; set; }
    public UserPermission Permission { get; set; }
}
