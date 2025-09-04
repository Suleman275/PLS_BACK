using FastEndpoints;
using FastEndpoints.Security;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedKernel.Constants;
using System.Security.Cryptography;
using UserManagement.API.Configurations;
using UserManagement.API.Constants;
using UserManagement.API.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Auth;

public class LoginRequest {
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public class LoginResponse {
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
}

public class LoginValidator : Validator<LoginRequest> {
    public LoginValidator() {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

public class Login(AppDbContext dbContext, IOptionsSnapshot<JwtOptions> jwtOptions) : Endpoint<LoginRequest, LoginResponse> {
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public override void Configure() {
        Post("auth/login");
        Version(1);
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct) {
        var user = await dbContext.Users
        .Include(u => u.Permissions)
        .FirstOrDefaultAsync(u => u.Email == req.Email, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash) || !user.IsActive) {
            AddError(r => r.Email, "Invalid Credentials");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        if (!user.IsEmailVerified) {
            AddError(r => r.Email, "Email not verified");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        if (user.Role == UserRole.Admin) {
            var existingAssignments = await dbContext.PermissionAssignments
                .Where(p => p.UserId == user.Id)
                .ExecuteDeleteAsync(ct);

            foreach (var perm in DefaultPermissionGroups.AdminPermissions) {
                dbContext.PermissionAssignments.Add(new PermissionAssignment {
                    UserId = user.Id,
                    Permission = perm
                });
            }
        }

        var accessToken = JwtBearer.CreateToken(o => {
            o.SigningKey = _jwtOptions.SigningKey;
            o.Issuer = _jwtOptions.Issuer;
            o.Audience = _jwtOptions.Audience;
            o.ExpireAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpiryMinutes);
            
            o.User.Claims.Add((ClaimNames.SubjectId, user.Id.ToString()));
            o.User.Claims.Add((ClaimNames.Email, user.Email));

            o.User.Roles.Add(user.Role.ToString());

            foreach (var permissionAssignment in user.Permissions) {
                o.User.Permissions.Add(permissionAssignment.Permission.ToString());
            }
        });

        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays);

        await dbContext.SaveChangesAsync(ct);

        await SendAsync(new LoginResponse {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        }, cancellation: ct);
    }
}

