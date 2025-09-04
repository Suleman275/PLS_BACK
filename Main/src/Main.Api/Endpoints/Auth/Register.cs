using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using UserManagement.API.Constants;
using UserManagement.API.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Auth;

public class RegisterRequest {
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string ConfirmPassword { get; set; } = default!;
    public UserRole Role { get; set; }
}

public class RegisterResponse {
    public string Message { get; set; } = default!;
}

public class RegisterValidator : Validator<RegisterRequest> {
    public RegisterValidator() {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Please enter a valid email address");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.");

        RuleFor(x => x.Password)
            .NotEmpty().MinimumLength(6); // Increase strength later

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Passwords do not match.");

        RuleFor(x => x.Role)
            .Must(r => r == UserRole.Student || r == UserRole.ImmigrationClient)
            .WithMessage("Only Student or ImmigrationClient roles are allowed.");
    }
}

public class Register(AppDbContext dbContext) : Endpoint<RegisterRequest, RegisterResponse> {
    public override void Configure() {
        Post("auth/register");
        Version(1);
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct) {
        var exists = await dbContext.Users.AnyAsync(u => u.Email == req.Email, ct);
        if (exists) {
            AddError(r => r.Email, "Email is already in use.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var defaultPermissions = req.Role switch {
            UserRole.Student => DefaultPermissionGroups.StudentPermissions,
            UserRole.ImmigrationClient => DefaultPermissionGroups.ImmigrationClientPermissions,
            _ => throw new InvalidOperationException("Unsupported role")
        };

        User newUser = req.Role switch {
            UserRole.Student => new StudentUser(),
            UserRole.ImmigrationClient => new ImmigrationClientUser(),
            _ => throw new InvalidOperationException("Unsupported role")
        };

        foreach (var permission in defaultPermissions) {
            newUser.Permissions.Add(new PermissionAssignment {
                Permission = permission
            });
        }

        var emailVerificationCode = Random.Shared.Next(100000, 999999).ToString();

        newUser.Email = req.Email;
        newUser.FirstName = req.FirstName;
        newUser.LastName = req.LastName;
        newUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password);
        newUser.Role = req.Role;
        newUser.IsActive = true;
        newUser.IsEmailVerified = false;
        newUser.EmailVerificationToken = emailVerificationCode;
        newUser.EmailVerificationTokenExpiry = DateTime.UtcNow.AddMinutes(15);

        dbContext.Users.Add(newUser);
        await dbContext.SaveChangesAsync(ct);

        await SendAsync(new RegisterResponse {
            Message = "Registration successful. Please verify your email using the 6-digit code."
        }, cancellation: ct);
    }
}