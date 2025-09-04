using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Students;

public class ListStudentsRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }

    [FromClaim(ClaimNames.Permissions)]
    public List<UserPermission> SubjectPermissions { get; set; } = [];

    [QueryParam]
    public int PageNumber { get; set; } = 1;

    [QueryParam]
    public int PageSize { get; set; } = 10;

    [QueryParam]
    public string? SearchTerm { get; set; }

    [QueryParam]
    public bool? IsActive { get; set; }
    
    [QueryParam]
    public Guid? RegisteredById { get; set; }

    [QueryParam]
    public DateTime? StartDate { get; set; }

    [QueryParam]
    public DateTime? EndDate { get; set; }
}

public class StudentUserListItem {
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public bool IsActive { get; set; }
    public Guid? RegisteredById { get; set; }
    public string? RegisteredByName { get; set; }
    public string? RegisteredByEmail { get; set; }
    public DateTime? RegistrationDate { get; set; }
    public Guid? AdmissionAssociateId { get; set; }
    public string? AdmissionAssociateName { get; set; }
    public Guid? CounselorId { get; set; }
    public string? CounselorName { get; set; }
}

public class ListStudentsResponse {
    public IEnumerable<StudentUserListItem> Students { get; set; } = Enumerable.Empty<StudentUserListItem>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ListStudentsRequestValidator : Validator<ListStudentsRequest> {
    public ListStudentsRequestValidator() {
        RuleFor(x => x.PageNumber)
         .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
         .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}

public class ListStudents(AppDbContext dbContext) : Endpoint<ListStudentsRequest, ListStudentsResponse> {
    public override void Configure() {
        Get("students");
        Version(1);
        Permissions(nameof(UserPermission.Students_Read));
    }

    public override async Task HandleAsync(ListStudentsRequest req, CancellationToken ct) {
        var query = dbContext.Users
            .OfType<StudentUser>()
            .Include(u => u.RegisteredBy)
            .Include(u => u.AdmissionAssociate)
            .Include(u => u.Counselor)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.SearchTerm)) {
            var searchTermLower = req.SearchTerm.ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(searchTermLower) ||
                u.FirstName.ToLower().Contains(searchTermLower) ||
                u.LastName.ToLower().Contains(searchTermLower));
        }

        if (req.IsActive.HasValue) {
            query = query.Where(u => u.IsActive == req.IsActive.Value);
        }

        if (req.RegisteredById.HasValue) {
            query = query.Where(u => u.RegisteredById == req.RegisteredById.Value);
        }

        if (req.StartDate.HasValue) {
            query = query.Where(u => u.RegistrationDate >= req.StartDate.Value.ToUniversalTime().Date);
        }

        if (req.EndDate.HasValue) {
            query = query.Where(u => u.RegistrationDate < req.EndDate.Value.ToUniversalTime().Date.AddDays(1));
        }

        var totalCount = await query.CountAsync(ct);

        query = query.OrderBy(u => u.LastName).ThenBy(u => u.FirstName);

        var students = await query
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(u => new StudentUserListItem {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                MiddleName = u.MiddleName,
                LastName = u.LastName,
                PhoneNumber = u.PhoneNumber,
                DateOfBirth = u.DateOfBirth,
                IsActive = u.IsActive,
                RegisteredById = u.RegisteredById,
                RegisteredByEmail = u.RegisteredBy != null ? u.RegisteredBy.Email : null,
                RegisteredByName = u.RegisteredBy != null
                    ? (u.RegisteredBy.FirstName + " " + u.RegisteredBy.LastName)
                    : null,
                RegistrationDate = u.RegistrationDate,
                AdmissionAssociateId = u.AdmissionAssociateId,
                AdmissionAssociateName = u.AdmissionAssociate != null
                    ? (u.AdmissionAssociate.FirstName + " " + u.AdmissionAssociate.LastName)
                    : null,
                CounselorId = u.CounselorId,
                CounselorName = u.Counselor != null
                    ? (u.Counselor.FirstName + " " + u.Counselor.LastName)
                    : null,
            })
            .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling((double)totalCount / req.PageSize);

        await SendOkAsync(new ListStudentsResponse {
            Students = students,
            TotalCount = totalCount,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
            TotalPages = totalPages
        }, ct);
    }
}