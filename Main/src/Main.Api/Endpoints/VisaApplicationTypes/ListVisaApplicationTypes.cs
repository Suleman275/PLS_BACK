using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.VisaApplicationTypes;

public class ListVisaApplicationTypesRequest {
    [QueryParam]
    public int PageNumber { get; set; } = 1;

    [QueryParam]
    public int PageSize { get; set; } = 10;

    [QueryParam]
    public string? SearchTerm { get; set; }
}

public class ListVisaApplicationTypesResponse {
    public IEnumerable<VisaApplicationTypeDto> VisaApplicationTypes { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class VisaApplicationTypeDto {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class ListVisaApplicationTypes(AppDbContext dbContext) : Endpoint<ListVisaApplicationTypesRequest, ListVisaApplicationTypesResponse> {
    public override void Configure() {
        Get("visa-application-types");
        Version(1);
        Permissions(nameof(UserPermission.UniversityTypes_Read));
    }

    public override async Task HandleAsync(ListVisaApplicationTypesRequest req, CancellationToken ct) {
        var query = dbContext.VisaApplicationTypes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.SearchTerm)) {
            query = query.Where(vat => vat.Name.Contains(req.SearchTerm) ||
                                       (vat.Description != null && vat.Description.Contains(req.SearchTerm)));
        }

        var totalCount = await query.CountAsync(ct);

        var visaApplicationTypes = await query
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(vat => new VisaApplicationTypeDto {
                Id = vat.Id,
                Name = vat.Name,
                Description = vat.Description,
            })
            .ToListAsync(ct);

        var response = new ListVisaApplicationTypesResponse {
            VisaApplicationTypes = visaApplicationTypes,
            TotalCount = totalCount,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize
        };

        await SendOkAsync(response, ct);
    }
}