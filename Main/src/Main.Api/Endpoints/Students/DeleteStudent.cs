using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Students;

public class DeleteStudentRequest {
    [FromRoute]
    public Guid Id { get; set; }
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; } 
}

public class DeleteStudentRequestValidator : Validator<DeleteStudentRequest> {
    public DeleteStudentRequestValidator() {
        RuleFor(x => x.Id)
         .NotEmpty().WithMessage("Student User ID is required.");
    }
}

public class DeleteStudent(AppDbContext dbContext) : Endpoint<DeleteStudentRequest, EmptyResponse> {
    public override void Configure() {
        Delete("students/{id}");
        Version(1);
        Permissions(nameof(UserPermission.Students_Delete));
    }

    public override async Task HandleAsync(DeleteStudentRequest req, CancellationToken ct) {
        var student = await dbContext.Users
                       .OfType<StudentUser>()
                       .FirstOrDefaultAsync(u => u.Id == req.Id, ct);

        if (student is null) {
            await SendNotFoundAsync(ct);
            return;
        }

        student.DeletedOn = DateTime.UtcNow;
        student.DeletedById = req.SubjectId;
        student.IsActive = false;

        await dbContext.SaveChangesAsync(ct);

        await SendNoContentAsync(ct);
    }
}