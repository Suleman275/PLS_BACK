// File: UserManagement.API/Endpoints/Partners/UpdatePartnerUser.cs
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;
using UserManagement.API.Constants;
using Main.Api.Data; // For DefaultPermissionGroups in example

namespace UserManagement.API.Endpoints.Partners;

// --- Request DTO ---
public class UpdatePartnerUserRequest {
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }

    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = default!;
    public string? Title { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public List<UserPermission> Permissions { get; set; } = [];
    public bool IsActive { get; set; }
}

public class UpdatePartnerUserResponse {
    public string Message { get; set; } = "Partner Updated Successfully";
}

public class UpdatePartnerUserRequestValidator : Validator<UpdatePartnerUserRequest> {
    public UpdatePartnerUserRequestValidator() {
        RuleFor(x => x.Id)
          .NotEmpty().WithMessage("Partner User ID is required.");

        RuleFor(x => x.Email)
          .NotEmpty().WithMessage("Email is required.")
          .EmailAddress().WithMessage("Invalid email format.")
          .MaximumLength(256).WithMessage("Email cannot exceed 256 characters.");

        RuleFor(x => x.FirstName)
          .NotEmpty().WithMessage("First name is required.")
          .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

        RuleFor(x => x.LastName)
          .NotEmpty().WithMessage("Last name is required.")
          .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

        RuleFor(x => x.MiddleName)
          .MaximumLength(100).WithMessage("Middle name cannot exceed 100 characters.");

        RuleFor(x => x.PhoneNumber)
          .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters.")
          .Matches(@"^\+?[0-9\s\-]{7,20}$").When(x => !string.IsNullOrEmpty(x.PhoneNumber)).WithMessage("Invalid phone number format.");

        RuleFor(x => x.DateOfBirth)
          .LessThan(DateTime.UtcNow.Date).WithMessage("Date of birth cannot be in the future.")
          .When(x => x.DateOfBirth.HasValue);
    }
}

public class UpdatePartner(AppDbContext dbContext) : Endpoint<UpdatePartnerUserRequest, UpdatePartnerUserResponse> {
    public override void Configure() {
        Put("partners/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.Partners_Update));
    }

    public override async Task HandleAsync(UpdatePartnerUserRequest req, CancellationToken ct) {
        var partner = await dbContext.Users.OfType<PartnerUser>()
          .FirstOrDefaultAsync(u => u.Id == req.Id, ct);

        if (partner == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        var emailConflict = await dbContext.Users.AsNoTracking()
          .AnyAsync(u => u.Email == req.Email && u.Id != req.Id, ct);

        if (emailConflict) {
            AddError(r => r.Email, "An account with this email already exists.");
            await SendErrorsAsync(409, ct); // 409 Conflict
            return;
        }

        partner.Email = req.Email;
        partner.FirstName = req.FirstName;
        partner.MiddleName = req.MiddleName;
        partner.LastName = req.LastName;
        partner.PhoneNumber = req.PhoneNumber;
        partner.DateOfBirth = req.DateOfBirth;
        partner.IsActive = req.IsActive;
        partner.LastModifiedOn = DateTime.UtcNow;
        partner.LastModifiedById = req.SubjectId;

        var existingAssignments = await dbContext.PermissionAssignments
                .Where(p => p.UserId == partner.Id)
                .ExecuteDeleteAsync(ct);

        foreach (var perm in req.Permissions) {
            dbContext.PermissionAssignments.Add(new PermissionAssignment {
                UserId = partner.Id,
                Permission = perm
            });
        }

        await dbContext.SaveChangesAsync(ct);

        await SendOkAsync(new UpdatePartnerUserResponse(), ct);
    }
}