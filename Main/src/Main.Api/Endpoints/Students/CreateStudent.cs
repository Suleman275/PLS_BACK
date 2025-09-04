using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Constants;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Students;

public class CreateStudentUserRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }

    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
}

public class CreateStudentUserResponse {
    public Guid Id { get; set; }
    public string Message { get; set; } = "Student Created Successfully";
}

public class CreateStudentUserRequestValidator : Validator<CreateStudentUserRequest> {
    public CreateStudentUserRequestValidator() {
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

public class CreateStudent(AppDbContext dbContext) : Endpoint<CreateStudentUserRequest, CreateStudentUserResponse> {
    public override void Configure() {
        Post("students");
        Version(1);
        Permissions(nameof(UserPermission.Students_Create));
    }

    public override async Task HandleAsync(CreateStudentUserRequest req, CancellationToken ct) {
        var existingUser = await dbContext.Users.AsNoTracking()
                .AnyAsync(u => u.Email == req.Email, ct);

        if (existingUser) {
            AddError(r => r.Email, "An account with this email already exists.");
            await SendErrorsAsync(409, ct);
            return;
        }

        var newStudent = new StudentUser {
            Email = req.Email,
            FirstName = req.FirstName,
            MiddleName = req.MiddleName,
            LastName = req.LastName,
            PhoneNumber = req.PhoneNumber,
            DateOfBirth = req.DateOfBirth,
            CreatedById = req.SubjectId,
            IsActive = true,
            IsEmailVerified = false,
            IsPhoneVerified = false
        };

        foreach (var permission in DefaultPermissionGroups.StudentPermissions) {
            newStudent.Permissions.Add(new PermissionAssignment {
                Permission = permission
            });
        }

        dbContext.Users.Add(newStudent);
        await dbContext.SaveChangesAsync(ct);

        await SendOkAsync(new CreateStudentUserResponse {
            Id = newStudent.Id,
        }, ct);
    }
}