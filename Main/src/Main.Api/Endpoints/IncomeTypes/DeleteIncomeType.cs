using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.IncomeTypes;

public class DeleteIncomeTypeRequest {
    [FromRoute]
    public Guid Id { get; set; }

    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
}

public class DeleteIncomeTypeRequestValidator : Validator<DeleteIncomeTypeRequest> {
    public DeleteIncomeTypeRequestValidator() {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Income type ID is required.");
    }
}

public class DeleteIncomeType(AppDbContext dbContext) : Endpoint<DeleteIncomeTypeRequest> {
    public override void Configure() {
        Delete("income-types/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.IncomeTypes_Delete));
    }

    public override async Task HandleAsync(DeleteIncomeTypeRequest req, CancellationToken ct) {
        var incomeType = await dbContext.IncomeTypes.FirstOrDefaultAsync(it => it.Id == req.Id, ct);

        if (incomeType == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        incomeType.DeletedOn = DateTime.UtcNow;
        incomeType.DeletedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendNoContentAsync(ct);
    }
}