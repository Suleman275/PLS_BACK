using SharedKernel.Models;
using UserManagement.API.Enums;

namespace UserManagement.API.Models;

public abstract class User : EntityBase {
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = default!;
    public virtual UserRole Role { get; set; }
    public virtual ICollection<PermissionAssignment> Permissions { get; set; } = [];

    // Authentication and security
    public string? PasswordHash { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    public string? ResetPasswordToken { get; set; }
    public DateTime? ResetPasswordTokenExpiry { get; set; }

    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiry { get; set; }

    public string? PhoneVerificationToken { get; set; }
    public DateTime? PhoneVerificationTokenExpiry { get; set; }

    // Verification and status
    public bool IsActive { get; set; } = true;
    public bool IsEmailVerified { get; set; } = false;
    public bool IsPhoneVerified { get; set; } = false;

    // Personal information
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? ProfilePictureUrl { get; set; }
}
