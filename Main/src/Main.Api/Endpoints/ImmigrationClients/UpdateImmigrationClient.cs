using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.ImmigrationClients;

public class UpdateImmigrationClientUserRequest {
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

    public Guid? LocationId { get; set; }
    public Guid? NationalityId { get; set; }
    public Guid? AdmissionAssociateId { get; set; }
    public Guid? CounselorId { get; set; }
    public Guid? RegisteredById { get; set; }
    public DateTime? RegistrationDate { get; set; }
    public Guid? ClientSourceId { get; set; }
    public Guid? SopWriterId { get; set; }
}

public class UpdateImmigrationClientUserResponse {
    public string Message { get; set; } = "Immigration Client Updated Successfully";
}

public class UpdateImmigrationClientUserRequestValidator : Validator<UpdateImmigrationClientUserRequest> {
    public UpdateImmigrationClientUserRequestValidator() {
        RuleFor(x => x.Id)
           .NotEmpty().WithMessage("Immigration Client User ID is required.");

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

public class UpdateImmigrationClientUser(AppDbContext dbContext) : Endpoint<UpdateImmigrationClientUserRequest, UpdateImmigrationClientUserResponse> {
    public override void Configure() {
        Put("immigration-clients/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.ImmigrationClients_Update), nameof(UserPermission.ImmigrationClients_Own_Update));
    }

    public override async Task HandleAsync(UpdateImmigrationClientUserRequest req, CancellationToken ct) {
        if (req.LocationId.HasValue && !await dbContext.Locations.AnyAsync(l => l.Id == req.LocationId.Value, ct)) {
            AddError(r => r.LocationId, "Specified Location does not exist.");
        }

        if (req.NationalityId.HasValue && !await dbContext.Nationalities.AnyAsync(n => n.Id == req.NationalityId.Value, ct)) {
            AddError(r => r.NationalityId, "Specified Nationality does not exist.");
        }

        if (req.AdmissionAssociateId.HasValue && !await dbContext.Users.OfType<EmployeeUser>().AnyAsync(e => e.Id == req.AdmissionAssociateId.Value, ct)) {
            AddError(r => r.AdmissionAssociateId, "Specified Admission Associate does not exist or is not an EmployeeUser.");
        }

        if (req.CounselorId.HasValue && !await dbContext.Users.OfType<EmployeeUser>().AnyAsync(e => e.Id == req.CounselorId.Value, ct)) {
            AddError(r => r.CounselorId, "Specified Counselor does not exist or is not an EmployeeUser.");
        }

        if (req.RegisteredById.HasValue && !await dbContext.Users.OfType<EmployeeUser>().AnyAsync(e => e.Id == req.RegisteredById.Value, ct)) {
            AddError(r => r.RegisteredById, "Specified RegisteredById does not exist or is not an EmployeeUser.");
        }

        if (req.ClientSourceId.HasValue && !await dbContext.ClientSources.AnyAsync(n => n.Id == req.ClientSourceId.Value, ct)) {
            AddError(r => r.ClientSourceId, "Specified Client Source does not exist.");
        }
        
        if (req.SopWriterId.HasValue && !await dbContext.ClientSources.AnyAsync(n => n.Id == req.SopWriterId.Value, ct)) {
            AddError(r => r.SopWriterId, "Specified Sop Writer does not exist.");
        }

        if (ValidationFailures.Count > 0) {
            await SendErrorsAsync(400, ct);
            return;
        }

        var immigrationClient = await dbContext.Users
            .OfType<ImmigrationClientUser>()
            .FirstOrDefaultAsync(u => u.Id == req.Id, ct);

        if (immigrationClient == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        var hasStudentsUpdatePermission = req.SubjectPermissions.Contains(UserPermission.Students_Update);
        var hasStudentsOwnUpdatePermission = req.SubjectPermissions.Contains(UserPermission.Students_Own_Update);

        if (!hasStudentsUpdatePermission && hasStudentsOwnUpdatePermission) {
            var isAssigned = immigrationClient.AdmissionAssociateId == req.SubjectId || immigrationClient.CounselorId == req.SubjectId;

            if (!isAssigned) {
                await SendForbiddenAsync(ct);
                return;
            }
        }

        var emailConflict = await dbContext.Users.AsNoTracking()
           .AnyAsync(u => u.Email == req.Email && u.Id != req.Id, ct);

        if (emailConflict) {
            AddError(r => r.Email, "An account with this email already exists.");
            await SendErrorsAsync(409, ct);
            return;
        }

        immigrationClient.Email = req.Email;
        immigrationClient.FirstName = req.FirstName;
        immigrationClient.MiddleName = req.MiddleName;
        immigrationClient.LastName = req.LastName;
        immigrationClient.PhoneNumber = req.PhoneNumber;
        immigrationClient.DateOfBirth = req.DateOfBirth;
        immigrationClient.IsActive = req.IsActive;
        immigrationClient.LocationId = req.LocationId;
        immigrationClient.NationalityId = req.NationalityId;
        immigrationClient.AdmissionAssociateId = req.AdmissionAssociateId;
        immigrationClient.CounselorId = req.CounselorId;
        immigrationClient.RegisteredById = req.RegisteredById;
        immigrationClient.RegistrationDate = req.RegistrationDate;
        immigrationClient.ClientSourceId = req.ClientSourceId;
        immigrationClient.LastModifiedOn = DateTime.UtcNow;
        immigrationClient.LastModifiedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendOkAsync(new UpdateImmigrationClientUserResponse(), ct);
    }
}