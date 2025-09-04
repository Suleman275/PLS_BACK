using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Constants;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Employees;

public class UpdateEmployeeUserRequest {
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

public class UpdateEmployeeUserResponse {
    public string Message { get; set; } = "Employee Updated Successfully";
}

public class UpdateEmployeeUserRequestValidator : Validator<UpdateEmployeeUserRequest> {
    public UpdateEmployeeUserRequestValidator() {
        RuleFor(x => x.Id)
         .NotEmpty().WithMessage("Employee User ID is required.");

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

        RuleFor(x => x.Title)
         .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

        RuleFor(x => x.PhoneNumber)
         .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters.")
         .Matches(@"^\+?[0-9\s\-]{7,20}$").When(x => !string.IsNullOrEmpty(x.PhoneNumber)).WithMessage("Invalid phone number format.");

        RuleFor(x => x.DateOfBirth)
         .LessThan(DateTime.UtcNow.Date).WithMessage("Date of birth cannot be in the future.")
         .When(x => x.DateOfBirth.HasValue);
    }
}

public class UpdateEmployee(AppDbContext dbContext) : Endpoint<UpdateEmployeeUserRequest, UpdateEmployeeUserResponse> {
    public override void Configure() {
        Put("employees/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.Employees_Update));
    }

    public override async Task HandleAsync(UpdateEmployeeUserRequest req, CancellationToken ct) {
        var employee = await dbContext.Users
            .OfType<EmployeeUser>()
            .Include(u => u.Permissions)
            .FirstOrDefaultAsync(u => u.Id == req.Id, ct);

        if (employee == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        var emailConflict = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == req.Email && u.Id != req.Id, ct);

        if (emailConflict) {
            AddError(r => r.Email, "An account with this email already exists.");
            await SendErrorsAsync(409, ct);
            return;
        }

        employee.Email = req.Email;
        employee.FirstName = req.FirstName;
        employee.MiddleName = req.MiddleName;
        employee.LastName = req.LastName;
        employee.Title = req.Title;
        employee.PhoneNumber = req.PhoneNumber;
        employee.DateOfBirth = req.DateOfBirth;
        employee.IsActive = req.IsActive;
        employee.LastModifiedOn = DateTime.UtcNow;
        employee.LastModifiedById = req.SubjectId;


        var existingAssignments = await dbContext.PermissionAssignments
                .Where(p => p.UserId == employee.Id)
                .ExecuteDeleteAsync(ct);

        foreach (var perm in req.Permissions) {
            dbContext.PermissionAssignments.Add(new PermissionAssignment {
                UserId = employee.Id,
                Permission = perm
            });
        }

        await dbContext.SaveChangesAsync(ct);

        await SendOkAsync(new UpdateEmployeeUserResponse(), ct);
    }
}