using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Constants;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Partners;

public class CreatePartnerUserRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; } 

    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? ProfilePictureUrl { get; set; }
}

public class CreatePartnerUserResponse {
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Message { get; set; } = "Partner Created Successfully";
}

public class CreatePartnerUserRequestValidator : Validator<CreatePartnerUserRequest> {
    public CreatePartnerUserRequestValidator() {
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

public class CreatePartnerUser(AppDbContext dbContext) : Endpoint<CreatePartnerUserRequest, CreatePartnerUserResponse> {
    public override void Configure() {
        Post("partners");
        Version(1);
        Permissions(nameof(UserPermission.Partners_Create)); // Assumes a permission for creating partners
        Summary(s => {
            s.Summary = "Creates a new partner user record";
            s.Description = "Adds a new partner user to the system with their personal details.";
            s.ExampleRequest = new CreatePartnerUserRequest {
                Email = "patricia.green@example.com",
                FirstName = "Patricia",
                LastName = "Green",
                PhoneNumber = "+40723456789",
                DateOfBirth = new DateTime(1988, 11, 25, 0, 0, 0, DateTimeKind.Utc),
            };
            s.ResponseExamples[201] = new CreatePartnerUserResponse {
                Id = Guid.NewGuid(),
                Email = "patricia.green@example.com",
                FirstName = "Patricia",
                LastName = "Green",
                Message = "Partner Created Successfully"
            };
        });
    }

    public override async Task HandleAsync(CreatePartnerUserRequest req, CancellationToken ct) {
        var existingUser = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == req.Email, ct);

        if (existingUser) {
            AddError(r => r.Email, "An account with this email already exists.");
            await SendErrorsAsync(409, ct); 
            return;
        }

        var newPartner = new PartnerUser {
            Email = req.Email,
            FirstName = req.FirstName,
            MiddleName = req.MiddleName,
            LastName = req.LastName,
            PhoneNumber = req.PhoneNumber,
            DateOfBirth = req.DateOfBirth,
            ProfilePictureUrl = req.ProfilePictureUrl,
            CreatedById = req.SubjectId,
            IsActive = true,
            IsEmailVerified = false,
            IsPhoneVerified = false
        };

        foreach (var permission in DefaultPermissionGroups.EmployeePermissions) {
            newPartner.Permissions.Add(new PermissionAssignment {
                Permission = permission
            });
        }

        dbContext.Users.Add(newPartner); 
        await dbContext.SaveChangesAsync(ct);

        await SendAsync(new CreatePartnerUserResponse {
            Id = newPartner.Id,
            Email = newPartner.Email,
            FirstName = newPartner.FirstName,
            LastName = newPartner.LastName
        }, 201, ct);
    }
}