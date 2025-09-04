using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.Partners;

public class ListPartnersRequest {
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

public class PartnerUserListItem {
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = default!;
    public UserRole Role { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? ProfilePictureUrl { get; set; }

    public IEnumerable<UserPermission> Permissions { get; set; } = [];
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsPhoneVerified { get; set; }
}

// --- Response DTO ---
public class ListPartnersResponse {
    public IEnumerable<PartnerUserListItem> Partners { get; set; } = Enumerable.Empty<PartnerUserListItem>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ListPartnersRequestValidator : Validator<ListPartnersRequest> {
    public ListPartnersRequestValidator() {
        RuleFor(x => x.PageNumber)
          .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
          .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}

public class ListPartners : Endpoint<ListPartnersRequest, ListPartnersResponse> {
    private readonly AppDbContext _dbContext;

    public ListPartners(AppDbContext dbContext) {
        _dbContext = dbContext;
    }

    public override void Configure() {
        Get("partners");
        Version(1);
        Permissions(nameof(UserPermission.Partners_Read));
    }

    public override async Task HandleAsync(ListPartnersRequest req, CancellationToken ct) {
        var query = _dbContext.Users.OfType<PartnerUser>().AsQueryable();

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

        var totalCount = await query.CountAsync(ct);

        query = query.OrderBy(u => u.LastName).ThenBy(u => u.FirstName); // Order by name

        var partnerUsers = await query
          .Skip((req.PageNumber - 1) * req.PageSize)
          .Take(req.PageSize)
          .Select(u => new PartnerUserListItem {
              Id = u.Id,
              Email = u.Email,
              FirstName = u.FirstName,
              MiddleName = u.MiddleName,
              LastName = u.LastName,
              Role = u.Role,
              PhoneNumber = u.PhoneNumber,
              DateOfBirth = u.DateOfBirth,
              ProfilePictureUrl = u.ProfilePictureUrl,
              Permissions = u.Permissions.Select(p => p.Permission),
              IsActive = u.IsActive,
              IsEmailVerified = u.IsEmailVerified,
              IsPhoneVerified = u.IsPhoneVerified,
          })
          .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling((double)totalCount / req.PageSize);

        await SendOkAsync(new ListPartnersResponse {
            Partners = partnerUsers,
            TotalCount = totalCount,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
            TotalPages = totalPages
        }, ct);
    }
}