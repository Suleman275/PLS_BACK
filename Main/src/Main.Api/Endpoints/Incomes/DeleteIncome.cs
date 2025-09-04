using FastEndpoints;
using FastEndpoints.Security;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.Incomes;

public class DeleteIncomeRequest {
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
}

public class DeleteIncomeRequestValidator : Validator<DeleteIncomeRequest> {
    public DeleteIncomeRequestValidator() {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Income ID is required.");
    }
}

public class DeleteIncome(AppDbContext dbContext) : Endpoint<DeleteIncomeRequest> {
    public override void Configure() {
        Delete("incomes/{Id}");
        Version(1);
        Permissions(nameof(UserPermission.Incomes_Delete)); // Requires permission to delete
        // If you have a specific delete permission, use that: Permissions(UserPermission.Incomes_Delete.ToString());

        Summary(s => {
            s.Summary = "Deletes an income record";
            s.Description = "Removes an income record from the system by its ID.";
            s.ExampleRequest = new DeleteIncomeRequest { Id = Guid.NewGuid() };
            s.Responses[204] = "Income record deleted successfully.";
            s.Responses[404] = "Income record not found.";
            s.Responses[403] = "Forbidden: User does not have permission or ownership.";
        });
    }

    public override async Task HandleAsync(DeleteIncomeRequest req, CancellationToken ct) {
        var income = await dbContext.Incomes
                                      .FirstOrDefaultAsync(i => i.Id == req.Id, ct);

        if (income == null) {
            await SendNotFoundAsync(ct);
            return;
        }

        income.DeletedOn = DateTime.UtcNow;
        income.DeletedById = req.SubjectId;

        await dbContext.SaveChangesAsync(ct);

        await SendNoContentAsync(ct);
    }
}