// File: UserManagement.API/Endpoints/Partners/DeletePartner.cs
using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Partners;

// --- Request DTO ---
public class DeletePartnerRequest {
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; } // Current user performing the deletion
}

// --- Request Validator ---
public class DeletePartnerRequestValidator : Validator<DeletePartnerRequest> {
    public DeletePartnerRequestValidator() {
        RuleFor(x => x.Id)
          .NotEmpty().WithMessage("Partner User ID is required.");
    }
}

// --- Endpoint Implementation ---
public class DeletePartner(AppDbContext dbContext) : Endpoint<DeletePartnerRequest, EmptyResponse> {
    public override void Configure() {
        Delete("partners/{id}");
        Version(1);
        Permissions(nameof(UserPermission.Partners_Delete)); // Requires permission to delete partners
        Summary(s => {
            s.Summary = "Soft deletes a partner user record";
            s.Description = "Marks a partner user record as deleted in the system by its ID.";
            s.ExampleRequest = new DeletePartnerRequest { Id = Guid.NewGuid() };
            s.Responses[204] = "Partner user record soft deleted successfully.";
            s.Responses[404] = "Partner user record not found.";
            s.Responses[403] = "Forbidden: User does not have permission.";
        });
    }

    public override async Task HandleAsync(DeletePartnerRequest req, CancellationToken ct) {
        var partner = await dbContext.Users
            .OfType<PartnerUser>()
            .FirstOrDefaultAsync(u => u.Id == req.Id && u.DeletedOn == null, ct);

        if (partner is null) {
            await SendNotFoundAsync(ct);
            return;
        }

        // Perform soft delete
        partner.DeletedOn = DateTime.UtcNow;
        partner.DeletedById = req.SubjectId;
        partner.IsActive = false; // Mark as inactive upon deletion

        await dbContext.SaveChangesAsync(ct);

        await SendNoContentAsync(ct); // 204 No Content typically for successful delete
    }
}