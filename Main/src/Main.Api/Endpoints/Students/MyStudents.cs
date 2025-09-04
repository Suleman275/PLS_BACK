using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Students;

public class MyStudentsRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }

    [QueryParam]
    public int PageNumber { get; set; } = 1;

    [QueryParam]
    public int PageSize { get; set; } = 10;
}

public class MyStudentListItem {
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string AssignmentType { get; set; } = default!;
}

public class MyStudentsResponse {
    public IEnumerable<MyStudentListItem> Students { get; set; } = Enumerable.Empty<MyStudentListItem>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class MyStudentsRequestValidator : Validator<MyStudentsRequest> {
    public MyStudentsRequestValidator() {
        RuleFor(x => x.PageNumber)
           .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
           .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}

public class MyStudents(AppDbContext dbContext) : Endpoint<MyStudentsRequest, MyStudentsResponse> {
    public override void Configure() {
        Get("students/my");
        Version(1);
        Permissions(nameof(UserPermission.Students_Own_Read));
    }

    public override async Task HandleAsync(MyStudentsRequest req, CancellationToken ct) {
        var query = dbContext.Users
            .OfType<StudentUser>()
            .Where(s =>
                s.AdmissionAssociateId == req.SubjectId ||
                s.CounselorId == req.SubjectId ||
                s.SopWriterId == req.SubjectId)
            .AsQueryable();

        var totalCount = await query.CountAsync(ct);

        query = query.OrderBy(s => s.LastName).ThenBy(s => s.FirstName);

        var students = await query
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToListAsync(ct);

        var studentListItems = students.Select(s => {
            string assignmentType = string.Empty;

            if (s.AdmissionAssociateId == req.SubjectId) {
                assignmentType = "Admission Associate";
            }
            else if (s.CounselorId == req.SubjectId) {
                assignmentType = "Counselor";
            }
            else if (s.SopWriterId == req.SubjectId) {
                assignmentType = "SOP Writer";
            }

            return new MyStudentListItem {
                Id = s.Id,
                Email = s.Email,
                FirstName = s.FirstName,
                LastName = s.LastName,
                AssignmentType = assignmentType
            };
        });

        var totalPages = (int)Math.Ceiling((double)totalCount / req.PageSize);

        await SendOkAsync(new MyStudentsResponse {
            Students = studentListItems,
            TotalCount = totalCount,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
            TotalPages = totalPages
        }, ct);
    }
}