using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.ProgramTypes;

public class ListAllProgramTypes(AppDbContext dbContext) : EndpointWithoutRequest<List<ListAllProgramTypes.ProgramTypeResponse>> {
    public override void Configure() {
        Get("program-types/all");
        Version(1);
        Permissions(nameof(UserPermission.ProgramTypes_Read));
    }

    public override async Task HandleAsync(CancellationToken ct) {
        var programTypes = await dbContext.ProgramTypes
            .AsNoTracking()
            .OrderBy(pt => pt.Name)
            .Select(pt => new ProgramTypeResponse {
                Id = pt.Id,
                Name = pt.Name,
            })
            .ToListAsync(ct);

        await SendOkAsync(programTypes, ct);
    }

    public class ProgramTypeResponse {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
    }
}