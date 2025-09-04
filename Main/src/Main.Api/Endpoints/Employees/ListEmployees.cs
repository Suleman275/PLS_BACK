using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.EmployeeUsers;

public class ListEmployeeUsersRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }

    [QueryParam]
    public int PageNumber { get; set; } = 1;

    [QueryParam]
    public int PageSize { get; set; } = 10;

    [QueryParam]
    public string? SearchTerm { get; set; }

    [QueryParam]
    public bool? IsActive { get; set; }
}

public class EmployeeUserListItem {
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = default!;
    public string? Title { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public IEnumerable<UserPermission> Permissions { get; set; } = [];
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsPhoneVerified { get; set; }
}

public class ListEmployeeUsersResponse {
    public IEnumerable<EmployeeUserListItem> Employees { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ListEmployeeUsersRequestValidator : Validator<ListEmployeeUsersRequest> {
    public ListEmployeeUsersRequestValidator() {
        RuleFor(x => x.PageNumber)
          .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
          .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}

public class ListEmployeeUsers(AppDbContext dbContext) : Endpoint<ListEmployeeUsersRequest, ListEmployeeUsersResponse> {
    public override void Configure() {
        Get("employees");
        Version(1);
        Permissions(nameof(UserPermission.Employees_Read));
    }

    public override async Task HandleAsync(ListEmployeeUsersRequest req, CancellationToken ct) {
        var query = dbContext.Users.OfType<EmployeeUser>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.SearchTerm)) {
            var searchTermLower = req.SearchTerm.ToLower();
            query = query.Where(u =>
              u.Email.ToLower().Contains(searchTermLower) ||
              u.FirstName.ToLower().Contains(searchTermLower) ||
              u.LastName.ToLower().Contains(searchTermLower) ||
              (u.Title != null && u.Title.ToLower().Contains(searchTermLower)));
        }

        if (req.IsActive.HasValue) {
            query = query.Where(u => u.IsActive == req.IsActive.Value);
        }

        var totalCount = await query.CountAsync(ct);

        query = query.OrderBy(u => u.LastName).ThenBy(u => u.FirstName);

        var employeeUsers = await query
              .Skip((req.PageNumber - 1) * req.PageSize)
              .Take(req.PageSize)
              .Select(u => new EmployeeUserListItem {
                  Id = u.Id,
                  Email = u.Email,
                  FirstName = u.FirstName,
                  MiddleName = u.MiddleName,
                  LastName = u.LastName,
                  Title = u.Title,
                  PhoneNumber = u.PhoneNumber,
                  DateOfBirth = u.DateOfBirth,
                  Permissions = u.Permissions.Select(p => p.Permission),
                  IsActive = u.IsActive,
                  IsEmailVerified = u.IsEmailVerified,
                  IsPhoneVerified = u.IsPhoneVerified,
              })
              .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling((double)totalCount / req.PageSize);

        await SendOkAsync(new ListEmployeeUsersResponse {
            Employees = employeeUsers,
            TotalCount = totalCount,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
            TotalPages = totalPages
        }, ct);
    }
}