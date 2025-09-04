using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace UserManagement.API.Endpoints.Public;

public class NationalityListItemResponse {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
}

public class ListNationalities(AppDbContext dbContext) : EndpointWithoutRequest<List<NationalityListItemResponse>> {
    public override void Configure() {
        Get("public/nationalities"); 
        Version(1); 
        AllowAnonymous(); 
    }

    public override async Task HandleAsync(CancellationToken ct) {
        var nationalities = await dbContext.Nationalities
            .OrderBy(n => n.Name)
            .Select(n => new NationalityListItemResponse {
                Id = n.Id,
                Name = n.Name
            })
            .ToListAsync(ct); 

        await SendOkAsync(nationalities, ct);
    }
}