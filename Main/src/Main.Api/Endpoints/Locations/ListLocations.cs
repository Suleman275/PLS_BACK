using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.Locations;

public class ListLocationsRequest {
    [QueryParam]
    public int PageNumber { get; set; } = 1;
    [QueryParam]
    public int PageSize { get; set; } = 10;
    [QueryParam]
    public string? SearchTerm { get; set; }
}

public class LocationDto {
    public Guid Id { get; set; }
    public string City { get; set; } = default!;
    public string Country { get; set; } = default!;
}

public class ListLocationsResponse {
    public IEnumerable<LocationDto> Locations { get; set; } = [];
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class ListLocations(AppDbContext dbContext) : Endpoint<ListLocationsRequest, ListLocationsResponse> {
    public override void Configure() {
        Get("locations");
        Version(1);
        Permissions(nameof(UserPermission.Locations_Read));
    }

    public override async Task HandleAsync(ListLocationsRequest req, CancellationToken ct) {
        req.PageNumber = Math.Max(1, req.PageNumber);
        req.PageSize = Math.Max(1, req.PageSize);

        var query = dbContext.Locations.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(req.SearchTerm)) {
            var searchTermLower = req.SearchTerm.ToLower();
            query = query.Where(l =>
                l.City.ToLower().Contains(searchTermLower) ||
                l.Country.ToLower().Contains(searchTermLower)
            );
        }

        var totalCount = await query.CountAsync(ct);

        var totalPages = (int)Math.Ceiling((double)totalCount / req.PageSize);

        var locations = await query
            .OrderBy(l => l.Country)
            .ThenBy(l => l.City)
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(l => new LocationDto {
                Id = l.Id,
                City = l.City,
                Country = l.Country,
            })
            .ToListAsync(ct);

        await SendAsync(new ListLocationsResponse {
            Locations = locations,
            TotalCount = totalCount,
            TotalPages = totalPages,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize
        }, cancellation: ct);
    }
}
