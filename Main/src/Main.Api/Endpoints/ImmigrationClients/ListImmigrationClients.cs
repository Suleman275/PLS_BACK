using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.ImmigrationClients;

public class ListImmigrationClientsRequest {
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

public class ImmigrationClientUserListItem {
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

public class ListImmigrationClientsResponse {
    public IEnumerable<ImmigrationClientUserListItem> Clients { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ListImmigrationClientsRequestValidator : Validator<ListImmigrationClientsRequest> {
    public ListImmigrationClientsRequestValidator() {
        RuleFor(x => x.PageNumber)
          .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
          .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}

public class ListImmigrationClients(AppDbContext dbContext) : Endpoint<ListImmigrationClientsRequest, ListImmigrationClientsResponse> {
    public override void Configure() {
        Get("immigration-clients");
        Version(1);
        Permissions(nameof(UserPermission.ImmigrationClients_Read)); 
    }

    public override async Task HandleAsync(ListImmigrationClientsRequest req, CancellationToken ct) {
        var query = dbContext.Users
            .OfType<ImmigrationClientUser>()
            .Include(u => u.RegisteredBy)
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

        var clientUsers = await query
          .Skip((req.PageNumber - 1) * req.PageSize)
          .Take(req.PageSize)
          .Select(u => new ImmigrationClientUserListItem {
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

        await SendOkAsync(new ListImmigrationClientsResponse {
            Clients = clientUsers,
            TotalCount = totalCount,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
            TotalPages = totalPages
        }, ct);
    }
}