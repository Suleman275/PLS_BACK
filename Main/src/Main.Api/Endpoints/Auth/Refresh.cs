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

public class RefreshTokenRequest {
    public string RefreshToken { get; set; } = default!;
}

public class RefreshTokenResponse {
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
}

public class RefreshTokenValidator : Validator<RefreshTokenRequest> {
    public RefreshTokenValidator() {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}

public class RefreshToken(AppDbContext dbContext, IOptionsSnapshot<JwtOptions> jwtOptions) : Endpoint<RefreshTokenRequest, RefreshTokenResponse> {
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public override void Configure() {
        Post("auth/refresh-token");
        Version(1);
        AllowAnonymous(); 
    }

    public override async Task HandleAsync(RefreshTokenRequest req, CancellationToken ct) {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == req.RefreshToken, ct);

        if (user is null ||
            user.RefreshToken != req.RefreshToken || 
            user.RefreshTokenExpiry <= DateTime.UtcNow || 
            !user.IsActive || 
            !user.IsEmailVerified) 
        {
            AddError(r => r.RefreshToken, "Invalid or expired refresh token.");
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

        var newAccessToken = JwtBearer.CreateToken(o => {
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

        var newRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays);
        await dbContext.SaveChangesAsync(ct);

        await SendAsync(new RefreshTokenResponse {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        }, cancellation: ct);
    }
}