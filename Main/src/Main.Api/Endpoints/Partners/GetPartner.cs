//// File: UserManagement.API/Endpoints/Partners/GetPartnerById.cs
//using FastEndpoints;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using SharedKernel.Enums;
//using UserManagement.API.Data;
//using UserManagement.API.Enums;
//using UserManagement.API.Models;

//namespace UserManagement.API.Endpoints.Partners;

//// --- Request DTO ---
//public class GetPartnerByIdRequest {
//    [FromRoute]
//    public Guid Id { get; set; }
//}

//// --- Response DTO ---
//public class PartnerDetailsResponse {
//    public Guid Id { get; set; }
//    public string Email { get; set; } = default!;
//    public string FirstName { get; set; } = default!;
//    public string? MiddleName { get; set; }
//    public string LastName { get; set; } = default!;
//    public UserRole Role { get; set; }
//    public UserPermission Permissions { get; set; }
//    public bool IsActive { get; set; }
//    public bool IsEmailVerified { get; set; }
//    public bool IsPhoneVerified { get; set; }
//    public string? PhoneNumber { get; set; }
//    public DateTime? DateOfBirth { get; set; }
//    public string? ProfilePictureUrl { get; set; }
//    public DateTime CreatedOn { get; set; }
//    public Guid CreatedById { get; set; }
//    public DateTime? LastModifiedOn { get; set; }
//    public Guid? LastModifiedById { get; set; }
//}

//// --- Endpoint Implementation ---
//public class GetPartnerById(AppDbContext dbContext) : Endpoint<GetPartnerByIdRequest, PartnerDetailsResponse> {
//    public override void Configure() {
//        Get("partners/{Id}");
//        Version(1);
//        Permissions(nameof(UserPermission.Partners_Read)); // Assumes permission to view partners
//    }

//    private void Permissions(object value) {
//        throw new NotImplementedException();
//    }

//    public override async Task HandleAsync(GetPartnerByIdRequest req, CancellationToken ct) {
//        var partner = await dbContext.Users
//            .OfType<PartnerUser>()
//            .Where(p => p.Id == req.Id)
//            .FirstOrDefaultAsync(ct);

//        if (partner is null) {
//            await SendNotFoundAsync(ct);
//            return;
//        }

//        var res = new PartnerDetailsResponse {
//            Id = partner.Id,
//            Email = partner.Email,
//            FirstName = partner.FirstName,
//            MiddleName = partner.MiddleName,
//            LastName = partner.LastName,
//            Role = partner.Role,
//            Permissions = partner.Permissions,
//            IsActive = partner.IsActive,
//            IsEmailVerified = partner.IsEmailVerified,
//            IsPhoneVerified = partner.IsPhoneVerified,
//            PhoneNumber = partner.PhoneNumber,
//            DateOfBirth = partner.DateOfBirth,
//            ProfilePictureUrl = partner.ProfilePictureUrl,
//            CreatedOn = partner.CreatedOn,
//            CreatedById = partner.CreatedById,
//            LastModifiedOn = partner.LastModifiedOn,
//            LastModifiedById = partner.LastModifiedById
//        };

//        await SendOkAsync(res, ct);
//    }
//}