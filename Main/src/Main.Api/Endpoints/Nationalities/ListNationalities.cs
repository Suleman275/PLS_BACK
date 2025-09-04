using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.Nationalities;

public class ListNationalitiesRequest {
    [QueryParam]
    public int PageNumber { get; set; } = 1;
    [QueryParam]
    public int PageSize { get; set; } = 10;
    [QueryParam]
    public string? SearchTerm { get; set; }
}

public class NationalityDto {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? TwoLetterCode { get; set; }
    public string? ThreeLetterCode { get; set; }
}

public class ListNationalitiesResponse {
    public IEnumerable<NationalityDto> Nationalities { get; set; } = [];
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class ListNationalities(AppDbContext dbContext) : Endpoint<ListNationalitiesRequest, ListNationalitiesResponse> {
    public override void Configure() {
        Get("nationalities");
        Version(1);
        Permissions(nameof(UserPermission.Nationalities_Read));
    }

    public override async Task HandleAsync(ListNationalitiesRequest req, CancellationToken ct) {
        req.PageNumber = Math.Max(1, req.PageNumber);
        req.PageSize = Math.Max(1, req.PageSize);

        var query = dbContext.Nationalities.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(req.SearchTerm)) {
            var searchTermLower = req.SearchTerm.ToLower();
            query = query.Where(n =>
                n.Name.ToLower().Contains(searchTermLower) ||
                (n.TwoLetterCode != null && n.TwoLetterCode.ToLower().Contains(searchTermLower)) ||
                (n.ThreeLetterCode != null && n.ThreeLetterCode.ToLower().Contains(searchTermLower))
            );
        }

        var totalCount = await query.CountAsync(ct);

        var totalPages = (int)Math.Ceiling((double)totalCount / req.PageSize);

        var nationalities = await query
            .OrderBy(n => n.Name)
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(n => new NationalityDto {
                Id = n.Id,
                Name = n.Name,
                TwoLetterCode = n.TwoLetterCode,
                ThreeLetterCode = n.ThreeLetterCode,
            })
            .ToListAsync(ct);

        await SendAsync(new ListNationalitiesResponse {
            Nationalities = nationalities,
            TotalCount = totalCount,
            TotalPages = totalPages,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize
        }, cancellation: ct);
    }
}