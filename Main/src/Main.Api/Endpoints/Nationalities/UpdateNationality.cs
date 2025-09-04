using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.Nationalities;

public class UpdateNationalityRequest {
    [FromRoute]
    public Guid Id { get; set; }

    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }

    public string Name { get; set; } = default!;
    public string? TwoLetterCode { get; set; }
    public string? ThreeLetterCode { get; set; }
}

public class UpdateNationalityResponse {
    public string Message { get; set; } = default!;
}

public class UpdateNationalityValidator : Validator<UpdateNationalityRequest> {
    public UpdateNationalityValidator() {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Nationality ID is required for update.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nationality name is required.")
            .MaximumLength(100).WithMessage("Nationality name cannot exceed 100 characters.");

        RuleFor(x => x.TwoLetterCode)
            .MaximumLength(2).WithMessage("Two-letter code cannot exceed 2 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.TwoLetterCode));

        RuleFor(x => x.ThreeLetterCode)
            .MaximumLength(3).WithMessage("Three-letter code cannot exceed 3 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.ThreeLetterCode));
    }
}

public class UpdateNationality(AppDbContext dbContext) : Endpoint<UpdateNationalityRequest, UpdateNationalityResponse> {
    public override void Configure() {
        Put("nationalities/{id}"); 
        Version(1); 
        Permissions(nameof(UserPermission.Nationalities_Update));
    }

    public override async Task HandleAsync(UpdateNationalityRequest req, CancellationToken ct) {
        var nationalityToUpdate = await dbContext.Nationalities.FirstOrDefaultAsync(n => n.Id == req.Id, ct);

        if (nationalityToUpdate == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        var existingNameConflict = await dbContext.Nationalities.AnyAsync(n => n.Id != req.Id && n.Name == req.Name, ct);

        if (existingNameConflict) {
            AddError(r => r.Name, "A nationality with this name already exists.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        if (!string.IsNullOrWhiteSpace(req.TwoLetterCode)) {
            var existingTwoLetterCodeConflict = await dbContext.Nationalities
                .AnyAsync(n => n.Id != req.Id && n.TwoLetterCode == req.TwoLetterCode, ct);
            if (existingTwoLetterCodeConflict) {
                AddError(r => r.TwoLetterCode, "A nationality with this two-letter code already exists.");
                await SendErrorsAsync(cancellation: ct);
                return;
            }
        }

        if (!string.IsNullOrWhiteSpace(req.ThreeLetterCode)) {
            var existingThreeLetterCodeConflict = await dbContext.Nationalities
                .AnyAsync(n => n.Id != req.Id && n.ThreeLetterCode == req.ThreeLetterCode, ct);
            if (existingThreeLetterCodeConflict) {
                AddError(r => r.ThreeLetterCode, "A nationality with this three-letter code already exists.");
                await SendErrorsAsync(cancellation: ct);
                return;
            }
        }

        nationalityToUpdate.Name = req.Name;
        nationalityToUpdate.TwoLetterCode = req.TwoLetterCode;
        nationalityToUpdate.ThreeLetterCode = req.ThreeLetterCode;
        nationalityToUpdate.LastModifiedById = req.SubjectId;
        nationalityToUpdate.LastModifiedOn = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        await SendOkAsync(new UpdateNationalityResponse {
            Message = $"Nationality '{nationalityToUpdate.Name}' (ID: {nationalityToUpdate.Id}) updated successfully.",
        }, cancellation: ct);
    }
}