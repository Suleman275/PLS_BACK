using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace UserManagement.API.Endpoints.Auth;

public class VerifyEmail(AppDbContext dbContext) : Endpoint<VerifyEmailRequest, VerifyEmailResponse> {
    public override void Configure() {
        Post("auth/verify-email");
        Version(1);
        AllowAnonymous();
    }

    public override async Task HandleAsync(VerifyEmailRequest req, CancellationToken ct) {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == req.Email, ct);

        if (user is null) {
            AddError(r => r.Email, "Email not found.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        if (user.IsEmailVerified) {
            await SendAsync(new VerifyEmailResponse {
                Message = "Email is already verified."
            }, cancellation: ct);
            return;
        }

        if (user.EmailVerificationToken != req.Code) {
            AddError(r => r.Code, "Invalid verification code.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        if (user.EmailVerificationTokenExpiry < DateTime.UtcNow) {
            AddError(r => r.Code, "Verification code has expired.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;

        await dbContext.SaveChangesAsync(ct);

        await SendAsync(new VerifyEmailResponse {
            Message = "Email verified successfully."
        }, cancellation: ct);
    }
}

public class VerifyEmailRequest {
    public string Email { get; set; } = default!;
    public string Code { get; set; } = default!;
}

public class VerifyEmailResponse {
    public string Message { get; set; } = default!;
}

public class VerifyEmailValidator : Validator<VerifyEmailRequest> {
    public VerifyEmailValidator() {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress();

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required.")
            .Length(6).WithMessage("Verification code must be 6 digits.")
            .Matches("^[0-9]{6}$").WithMessage("Code must be numeric.");
    }
}
