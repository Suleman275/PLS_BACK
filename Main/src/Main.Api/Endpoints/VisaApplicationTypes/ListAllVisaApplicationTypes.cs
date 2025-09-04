using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.VisaApplicationTypes;

public class ListAllVisaApplicationTypesRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
}

public class ListAllVisaApplicationTypesResponse {
    public IEnumerable<VisaApplicationTypeDto> VisaApplicationTypes { get; set; } = [];
}

public class ListAllVisaApplicationTypes(AppDbContext dbContext) : Endpoint<ListAllVisaApplicationTypesRequest, ListAllVisaApplicationTypesResponse> {
    public override void Configure() {
        Get("visa-application-types/all");
        Version(1);
        Permissions(nameof(UserPermission.VisaApplicationTypes_Read));
    }

    public override async Task HandleAsync(ListAllVisaApplicationTypesRequest req, CancellationToken ct) {
        var visaApplicationTypes = await dbContext.VisaApplicationTypes
            .Select(vat => new VisaApplicationTypeDto {
                Id = vat.Id,
                Name = vat.Name,
                Description = vat.Description,
            })
            .ToListAsync(ct);

        var response = new ListAllVisaApplicationTypesResponse {
            VisaApplicationTypes = visaApplicationTypes
        };

        await SendOkAsync(response, ct);
    }
}