using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Students;

public class UpdateStudentUserRequest {
    [FromRoute]
    public Guid Id { get; set; }

    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }

    [FromClaim(ClaimNames.Permissions)]
    public List<UserPermission> SubjectPermissions { get; set; } = [];

    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public bool IsActive { get; set; }
    public long StorageLimit { get; set; }

    public Guid? LocationId { get; set; }
    public Guid? NationalityId { get; set; }
    public Guid? AdmissionAssociateId { get; set; }
    public Guid? CounselorId { get; set; }
    public Guid? RegisteredById { get; set; }
    public DateTime? RegistrationDate { get; set; }
    public Guid? ClientSourceId { get; set; }
    public Guid? SopWriterId { get; set; }
}

public class UpdateStudentUserResponse {
    public string Message { get; set; } = "Student Updated Successfully";
}

public class UpdateStudentUserRequestValidator : Validator<UpdateStudentUserRequest> {
    public UpdateStudentUserRequestValidator() {

        RuleFor(x => x.Id)
           .NotEmpty().WithMessage("Student User ID is required.");

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

public class UpdateStudent(AppDbContext dbContext) : Endpoint<UpdateStudentUserRequest, UpdateStudentUserResponse> {
    public override void Configure() {
        Put("students/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.Students_Update), nameof(UserPermission.Students_Own_Update));
    }

    public override async Task HandleAsync(UpdateStudentUserRequest req, CancellationToken ct) {
        var student = await dbContext.Users.OfType<StudentUser>()
           .FirstOrDefaultAsync(u => u.Id == req.Id, ct);

        if (student == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        var emailConflict = await dbContext.Users.AsNoTracking()
           .AnyAsync(u => u.Email == req.Email && u.Id != req.Id, ct);

        if (emailConflict) {
            AddError(r => r.Email, "An account with this email already exists.");
            await SendErrorsAsync(409, ct);
            return;
        }

        var hasStudentsUpdatePermission = req.SubjectPermissions.Contains(UserPermission.Students_Update);
        var hasStudentsOwnUpdatePermission = req.SubjectPermissions.Contains(UserPermission.Students_Own_Update);

        if (!hasStudentsUpdatePermission && hasStudentsOwnUpdatePermission) {
            var isAssigned = student.AdmissionAssociateId == req.SubjectId || student.CounselorId == req.SubjectId;

            if (!isAssigned) {
                await SendForbiddenAsync(ct);
                return;
            }
        }

        student.Email = req.Email;
        student.FirstName = req.FirstName;
        student.MiddleName = req.MiddleName;
        student.LastName = req.LastName;
        student.PhoneNumber = req.PhoneNumber;
        student.DateOfBirth = req.DateOfBirth;
        student.IsActive = req.IsActive;
        student.StorageLimit = req.StorageLimit;
        student.LocationId = req.LocationId;
        student.NationalityId = req.NationalityId;
        student.AdmissionAssociateId = req.AdmissionAssociateId;
        student.CounselorId = req.CounselorId;
        student.RegisteredById = req.RegisteredById;
        student.RegistrationDate = req.RegistrationDate;
        student.ClientSourceId = req.ClientSourceId;
        student.SopWriterId = req.SopWriterId;

        student.LastModifiedOn = DateTime.UtcNow;
        student.LastModifiedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendOkAsync(new UpdateStudentUserResponse(), ct);
    }
}