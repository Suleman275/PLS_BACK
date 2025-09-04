using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;

namespace UserManagement.API.Endpoints.Dashboards.Portal;

public class TypesCountResponse {
    public int LocationsCount { get; set; }
    public int NationalitiesCount { get; set; }
    public int UniversityTypesCount { get; set; }
    public int ProgramTypesCount { get; set; }
    public int DocumentTypesCount { get; set; }
    public int ExpenseTypesCount { get; set; }
    public int IncomeTypesCount { get; set; }
    public int VisaApplicationTypesCount { get; set; }
    public int CurrenciesCount { get; set; }
    public int ClientSourcesCount { get; set; }
}

public class TypesCount(AppDbContext dbContext) : Endpoint<EmptyRequest, TypesCountResponse> {
    public override void Configure() {
        Get("dashboard/portal/types-count");
        Version(1);
        Permissions(UserPermission.Portal_Overview.ToString());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct) {
        var response = new TypesCountResponse {
            LocationsCount = await dbContext.Locations.CountAsync(),
            NationalitiesCount = await dbContext.Nationalities.CountAsync(),
            UniversityTypesCount = await dbContext.UniversityTypes.CountAsync(),
            ProgramTypesCount = await dbContext.ProgramTypes.CountAsync(),
            IncomeTypesCount = await dbContext.IncomeTypes.CountAsync(),
            ExpenseTypesCount = await dbContext.ExpenseTypes.CountAsync(),
            DocumentTypesCount = await dbContext.DocumentTypes.CountAsync(),
            VisaApplicationTypesCount = await dbContext.VisaApplications.CountAsync(),
            CurrenciesCount = await dbContext.Currencies.CountAsync(),
            ClientSourcesCount = await dbContext.ClientSources.CountAsync(),
        };

        await SendOkAsync(response);
    }
}
