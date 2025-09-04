using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.Currencies;

public class UpdateCurrencyRequest {
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!; 
    public string? Description { get; set; }
}

public class UpdateCurrencyResponse {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!; 
    public string? Description { get; set; }
}

public class UpdateCurrencyRequestValidator : Validator<UpdateCurrencyRequest> {
    public UpdateCurrencyRequestValidator() {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Currency ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Currency name is required.")
            .MaximumLength(100).WithMessage("Currency name cannot exceed 100 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Currency code is required.")
            .Length(3).WithMessage("Currency code must be 3 characters long (e.g., USD, EUR).")
            .Matches("^[A-Z]{3}$").WithMessage("Currency code must consist of 3 uppercase letters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}

public class UpdateCurrency(AppDbContext dbContext) : Endpoint<UpdateCurrencyRequest, UpdateCurrencyResponse> {
    public override void Configure() {
        Put("currencies/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.Currencies_Update));
    }

    public override async Task HandleAsync(UpdateCurrencyRequest req, CancellationToken ct) {
        var currency = await dbContext.Currencies.FirstOrDefaultAsync(c => c.Id == req.Id, ct);

        if (currency == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        currency.Name = req.Name;
        currency.Code = req.Code.ToUpper();
        currency.Description = req.Description;
        currency.LastModifiedOn = DateTime.UtcNow;
        currency.LastModifiedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendOkAsync(new UpdateCurrencyResponse {
            Id = currency.Id,
            Name = currency.Name,
            Code = currency.Code,
            Description = currency.Description,
        }, ct);
    }
}