using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace UserManagement.API.Endpoints.Auth;

public class ResendEmailVerificationRequest {
    public string Email { get; set; } = default!;
}

public class ResendEmailVerificationResponse {
    public string Message { get; set; } = default!;
}

public class ResendEmailVerificationValidator : Validator<ResendEmailVerificationRequest> {
    public ResendEmailVerificationValidator() {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress();
    }
}

public class ResendEmailVerificationToken(AppDbContext dbContext) : Endpoint<ResendEmailVerificationRequest, ResendEmailVerificationResponse> {
    public override void Configure() {
        Post("auth/resend-email-verification");
        Version(1);
        AllowAnonymous();
    }

    public override async Task HandleAsync(ResendEmailVerificationRequest req, CancellationToken ct) {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == req.Email, ct);

        if (user is null) {
            AddError(r => r.Email, "No user found with this email.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        if (user.IsEmailVerified) {
            AddError(r => r.Email, "Email is already verified.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        // Generate new 6-digit code
        var newCode = Random.Shared.Next(100000, 999999).ToString();

        user.EmailVerificationToken = newCode;
        user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddMinutes(15);

        await dbContext.SaveChangesAsync(ct);

        // Optional: send the code to user's email here
        // await emailService.SendVerificationCodeAsync(user.Email, newCode);

        await SendAsync(new ResendEmailVerificationResponse {
            Message = "Verification code resent. Please check your email."
        }, cancellation: ct);
    }
}
