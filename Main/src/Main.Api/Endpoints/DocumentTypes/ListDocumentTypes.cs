using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.DocumentTypes;

public class ListDocumentTypesRequest {
    [QueryParam]
    public int PageNumber { get; set; } = 1;

    [QueryParam]
    public int PageSize { get; set; } = 10;

    [QueryParam]
    public string? SearchTerm { get; set; }
}

public class ListDocumentTypesResponse {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class ListDocumentTypesPaginatedResponse {
    public IEnumerable<ListDocumentTypesResponse> Items { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

public class ListDocumentTypesRequestValidator : Validator<ListDocumentTypesRequest> {
    public ListDocumentTypesRequestValidator() {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}

public class ListDocumentTypes(AppDbContext dbContext) : Endpoint<ListDocumentTypesRequest, ListDocumentTypesPaginatedResponse> {
    public override void Configure() {
        Get("document-types");
        Version(1);
        Permissions(nameof(UserPermission.DocumentTypes_Read));
    }

    public override async Task HandleAsync(ListDocumentTypesRequest req, CancellationToken ct) {
        var query = dbContext.DocumentTypes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(req.SearchTerm)) {
            var searchTermLower = req.SearchTerm.ToLower();
            query = query.Where(dt =>
                dt.Name.ToLower().Contains(searchTermLower) ||
                (dt.Description != null && dt.Description.ToLower().Contains(searchTermLower))
            );
        }

        var totalCount = await query.CountAsync(ct);

        var documentTypes = await query
            .OrderBy(dt => dt.Name)
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(dt => new ListDocumentTypesResponse {
                Id = dt.Id,
                Name = dt.Name,
                Description = dt.Description
            })
            .ToListAsync(ct);

        var paginatedResponse = new ListDocumentTypesPaginatedResponse {
            Items = documentTypes,
            TotalCount = totalCount,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)req.PageSize)
        };

        await SendAsync(paginatedResponse, cancellation: ct);
    }
}

