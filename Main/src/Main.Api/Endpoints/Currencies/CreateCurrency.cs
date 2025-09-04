using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Currencies;

public class CreateCurrencyRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string? Description { get; set; }
}

public class CreateCurrencyResponse {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string? Description { get; set; }
}

public class CreateCurrencyRequestValidator : Validator<CreateCurrencyRequest> {
    public CreateCurrencyRequestValidator() {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Currency name is required.")
            .MaximumLength(100).WithMessage("Currency name cannot exceed 100 characters.");

        RuleFor(x => x.Code) // New validation for Code
            .NotEmpty().WithMessage("Currency code is required.")
            .Length(3).WithMessage("Currency code must be 3 characters long (e.g., USD, EUR).")
            .Matches("^[A-Z]{3}$").WithMessage("Currency code must consist of 3 uppercase letters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}

public class CreateCurrency(AppDbContext dbContext) : Endpoint<CreateCurrencyRequest, CreateCurrencyResponse> {
    public override void Configure() {
        Post("currencies");
        Version(1);
        Permissions(nameof(UserPermission.Currencies_Create));
    }

    public override async Task HandleAsync(CreateCurrencyRequest req, CancellationToken ct) {
        var newCurrency = new Currency {
            Name = req.Name,
            Code = req.Code.ToUpper(),
            Description = req.Description,
            CreatedById = req.SubjectId,
        };

        dbContext.Currencies.Add(newCurrency);

        await dbContext.SaveChangesAsync(ct);

        await SendAsync(new CreateCurrencyResponse {
            Id = newCurrency.Id,
            Name = newCurrency.Name,
            Code = newCurrency.Code,
            Description = newCurrency.Description,
        }, 201, ct);
    }
}