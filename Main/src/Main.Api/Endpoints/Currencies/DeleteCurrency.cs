using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Currencies;

public class DeleteCurrencyRequest {
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
}

public class DeleteCurrencyRequestValidator : Validator<DeleteCurrencyRequest> {
    public DeleteCurrencyRequestValidator() {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Currency ID is required.");
    }
}

public class DeleteCurrency(AppDbContext dbContext) : Endpoint<DeleteCurrencyRequest> {
    public override void Configure() {
        Delete("currencies/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.Currencies_Delete));
    }

    public override async Task HandleAsync(DeleteCurrencyRequest req, CancellationToken ct) {
        var currency = await dbContext.Currencies.FirstOrDefaultAsync(c => c.Id == req.Id, ct);

        if (currency == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        currency.DeletedOn = DateTime.UtcNow;
        currency.DeletedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendNoContentAsync(ct);
    }
}