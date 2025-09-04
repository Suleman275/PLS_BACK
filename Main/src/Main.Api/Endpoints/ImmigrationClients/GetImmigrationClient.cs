using FastEndpoints;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.ImmigrationClients;

public class GetImmigrationClientByIdRequest {
    [FromRoute]
    public Guid Id { get; set; }

    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }

    [FromClaim(ClaimNames.Permissions)]
    public List<UserPermission> SubjectPermissions { get; set; } = [];
}

public class ImmigrationClientDetailsResponse {
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

public class GetImmigrationClient(AppDbContext dbContext) : Endpoint<GetImmigrationClientByIdRequest, ImmigrationClientDetailsResponse> {
    public override void Configure() {
        Get("immigration-clients/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.ImmigrationClients_Read), nameof(UserPermission.ImmigrationClients_Own_Read)); 
    }

    public override async Task HandleAsync(GetImmigrationClientByIdRequest req, CancellationToken ct) {
        var immigrationClient = await dbContext.Users
            .OfType<ImmigrationClientUser>()
            .Include(ic => ic.Location)
            .Include(ic => ic.Nationality)
            .Include(ic => ic.AdmissionAssociate)
            .Include(ic => ic.Counselor)
            .Include(ic => ic.RegisteredBy)
            .Include(s => s.ClientSource)
            .Where(ic => ic.Id == req.Id)
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

        if (immigrationClient is null) {
            await SendNotFoundAsync(ct);
            return;
        }

        var hasClientsViewPermission = req.SubjectPermissions.Contains(UserPermission.ImmigrationClients_Read);
        var hasClientsOwnViewPermission = req.SubjectPermissions.Contains(UserPermission.ImmigrationClients_Own_Read);

        if (!hasClientsViewPermission && hasClientsOwnViewPermission) {
            var isAssigned = immigrationClient.AdmissionAssociateId == req.SubjectId ||
                immigrationClient.CounselorId == req.SubjectId ||
                immigrationClient.SopWriterId == req.SubjectId;

            if (!isAssigned) {
                await SendForbiddenAsync(ct);
                return;
            }
        }

        var res = new ImmigrationClientDetailsResponse {
            Id = immigrationClient.Id,
            Email = immigrationClient.Email,
            FirstName = immigrationClient.FirstName,
            MiddleName = immigrationClient.MiddleName,
            LastName = immigrationClient.LastName,
            IsActive = immigrationClient.IsActive,
            IsEmailVerified = immigrationClient.IsEmailVerified,
            IsPhoneVerified = immigrationClient.IsPhoneVerified,
            PhoneNumber = immigrationClient.PhoneNumber,
            DateOfBirth = immigrationClient.DateOfBirth,
            ProfilePictureUrl = immigrationClient.ProfilePictureUrl,
            CreatedOn = immigrationClient.CreatedOn,
            CreatedById = immigrationClient.CreatedById,
            LastModifiedOn = immigrationClient.LastModifiedOn,
            LastModifiedById = immigrationClient.LastModifiedById,
            LocationId = immigrationClient.LocationId,
            LocationName = immigrationClient.Location?.City + ", " + immigrationClient.Location?.Country,
            NationalityId = immigrationClient.NationalityId,
            NationalityName = immigrationClient.Nationality?.Name,
            AdmissionAssociateId = immigrationClient.AdmissionAssociateId,
            AdmissionAssociateFullName = immigrationClient.AdmissionAssociate != null ? $"{immigrationClient.AdmissionAssociate.FirstName} {immigrationClient.AdmissionAssociate.LastName}" : null,
            CounselorId = immigrationClient.CounselorId,
            CounselorFullName = immigrationClient.Counselor != null ? $"{immigrationClient.Counselor.FirstName} {immigrationClient.Counselor.LastName}" : null,
            SopWriterId = immigrationClient.SopWriterId,
            SopWriterFullName = immigrationClient.SopWriter != null ? $"{immigrationClient.SopWriter.FirstName} {immigrationClient.SopWriter.LastName}" : null,
            RegisteredById = immigrationClient.RegisteredById,
            RegisteredByFullName = immigrationClient.RegisteredBy != null ? $"{immigrationClient.RegisteredBy.FirstName} {immigrationClient.RegisteredBy.LastName}" : null,
            RegistrationDate = immigrationClient.RegistrationDate,
            ClientSourceId = immigrationClient.ClientSourceId,
            ClientSourceName = immigrationClient.ClientSource != null ? immigrationClient.ClientSource.Name : null,
            StorageLimit = immigrationClient.StorageLimit,
            StorageUsage = immigrationClient.StorageUsage,
        };

        await SendOkAsync(res, ct);
    }
}