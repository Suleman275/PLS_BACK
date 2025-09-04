using FastEndpoints;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Students;

public class GetStudentByIdRequest {
    [FromRoute]
    public Guid Id { get; set; }

    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }

    [FromClaim(ClaimNames.Permissions)]
    public List<UserPermission> SubjectPermissions { get; set; } = [];
}

public class StudentDetailsResponse {
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = default!;
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsPhoneVerified { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid CreatedById { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public Guid? LastModifiedById { get; set; }

    public Guid? LocationId { get; set; }
    public string? LocationName { get; set; }

    public Guid? NationalityId { get; set; }
    public string? NationalityName { get; set; }

    public Guid? AdmissionAssociateId { get; set; }
    public string? AdmissionAssociateFullName { get; set; }

    public Guid? CounselorId { get; set; }
    public string? CounselorFullName { get; set; }

    public Guid? SopWriterId { get; set; }
    public string? SopWriterFullName { get; set; }

    public Guid? RegisteredById { get; set; }
    public string? RegisteredByFullName { get; set; }
    public DateTime? RegistrationDate { get; set; }

    public Guid? ClientSourceId { get; set; }
    public string? ClientSourceName { get; set; }

    public long StorageUsage { get; set; }
    public long StorageLimit { get; set; }
}

public class GetStudent(AppDbContext dbContext) : Endpoint<GetStudentByIdRequest, StudentDetailsResponse> {
    public override void Configure() {
        Get("students/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.Students_Read), nameof(UserPermission.Students_Own_Read));
    }

    public override async Task HandleAsync(GetStudentByIdRequest req, CancellationToken ct) {
        var student = await dbContext.Users
            .OfType<StudentUser>()
            .Include(s => s.Location)
            .Include(s => s.Nationality)
            .Include(s => s.AdmissionAssociate)
            .Include(s => s.Counselor)
            .Include(s => s.SopWriter) 
            .Include(s => s.RegisteredBy)
            .Include(s => s.ClientSource)
            .Where(s => s.Id == req.Id)
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

        if (student is null) {
            await SendNotFoundAsync(ct);
            return;
        }

        var hasStudentsViewPermission = req.SubjectPermissions.Contains(UserPermission.Students_Read);
        var hasStudentsOwnViewPermission = req.SubjectPermissions.Contains(UserPermission.Students_Own_Read);

        if (!hasStudentsViewPermission && hasStudentsOwnViewPermission) {
            var isAssigned = student.AdmissionAssociateId == req.SubjectId ||
                student.CounselorId == req.SubjectId ||
                student.SopWriterId == req.SubjectId;

            if (!isAssigned) {
                await SendForbiddenAsync(ct);
                return;
            }
        }

        var res = new StudentDetailsResponse {
            Id = student.Id,
            Email = student.Email,
            FirstName = student.FirstName,
            MiddleName = student.MiddleName,
            LastName = student.LastName,
            IsActive = student.IsActive,
            IsEmailVerified = student.IsEmailVerified,
            IsPhoneVerified = student.IsPhoneVerified,
            PhoneNumber = student.PhoneNumber,
            DateOfBirth = student.DateOfBirth,
            ProfilePictureUrl = student.ProfilePictureUrl,
            CreatedOn = student.CreatedOn,
            CreatedById = student.CreatedById,
            LastModifiedOn = student.LastModifiedOn,
            LastModifiedById = student.LastModifiedById,
            LocationId = student.LocationId,
            LocationName = student.Location?.City + ", " + student.Location?.Country,
            NationalityId = student.NationalityId,
            NationalityName = student.Nationality?.Name,
            AdmissionAssociateId = student.AdmissionAssociateId,
            AdmissionAssociateFullName = student.AdmissionAssociate != null ? $"{student.AdmissionAssociate.FirstName} {student.AdmissionAssociate.LastName}" : null,
            CounselorId = student.CounselorId,
            CounselorFullName = student.Counselor != null ? $"{student.Counselor.FirstName} {student.Counselor.LastName}" : null,
            SopWriterId = student.SopWriterId,
            SopWriterFullName = student.SopWriter != null ? $"{student.SopWriter.FirstName} {student.SopWriter.LastName}" : null,
            RegisteredById = student.RegisteredById,
            RegisteredByFullName = student.RegisteredBy != null ? $"{student.RegisteredBy.FirstName} {student.RegisteredBy.LastName}" : null,
            RegistrationDate = student.RegistrationDate,
            ClientSourceId = student.ClientSourceId,
            ClientSourceName = student.ClientSource != null ? student.ClientSource.Name : null,
            StorageLimit = student.StorageLimit,
            StorageUsage = student.StorageUsage,
        };

        await SendOkAsync(res, ct);
    }
}