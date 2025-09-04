using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Nationalities;

public class CreateNationalityRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    public string Name { get; set; } = default!;
    public string? TwoLetterCode { get; set; }
    public string? ThreeLetterCode { get; set; }
}

public class CreateNationalityResponse {
    public string Message { get; set; } = default!;
    public Guid Id { get; set; } 
}

public class CreateNationalityValidator : Validator<CreateNationalityRequest> {
    public CreateNationalityValidator() {
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

public class CreateNationality(AppDbContext dbContext) : Endpoint<CreateNationalityRequest, CreateNationalityResponse> {
    public override void Configure() {
        Post("nationalities");
        Version(1); 
        Permissions(nameof(UserPermission.Nationalities_Create));
    }

    public override async Task HandleAsync(CreateNationalityRequest req, CancellationToken ct) {
        var existingNationality = await dbContext.Nationalities
            .FirstOrDefaultAsync(n => n.Name == req.Name, ct);

        if (existingNationality != null) {
            AddError(r => r.Name, "A nationality with this name already exists.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        if (!string.IsNullOrWhiteSpace(req.TwoLetterCode)) {
            var existingByTwoLetterCode = await dbContext.Nationalities
                .FirstOrDefaultAsync(n => n.TwoLetterCode == req.TwoLetterCode, ct);
            if (existingByTwoLetterCode != null) {
                AddError(r => r.TwoLetterCode, "A nationality with this two-letter code already exists.");
                await SendErrorsAsync(cancellation: ct);
                return;
            }
        }

        if (!string.IsNullOrWhiteSpace(req.ThreeLetterCode)) {
            var existingByThreeLetterCode = await dbContext.Nationalities
                .FirstOrDefaultAsync(n => n.ThreeLetterCode == req.ThreeLetterCode, ct);
            if (existingByThreeLetterCode != null) {
                AddError(r => r.ThreeLetterCode, "A nationality with this three-letter code already exists.");
                await SendErrorsAsync(cancellation: ct);
                return;
            }
        }


        var newNationality = new Nationality {
            Name = req.Name,
            TwoLetterCode = req.TwoLetterCode,
            ThreeLetterCode = req.ThreeLetterCode,
            CreatedById = req.SubjectId,
        };

        dbContext.Nationalities.Add(newNationality);

        await dbContext.SaveChangesAsync(ct);

        await SendAsync(new CreateNationalityResponse {
            Message = $"Nationality '{newNationality.Name}' created successfully with ID: {newNationality.Id}.",
            Id = newNationality.Id
        }, statusCode: 201, cancellation: ct);
    }
}