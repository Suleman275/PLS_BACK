using CsvHelper;
using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using System.Globalization;
using UserManagement.API.Enums;
using UserManagement.API.Models;

public class ImportIncomesRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    public IFormFile CsvFile { get; set; } = default!;
}

public class ImportIncomesResponse {
    public int ImportedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class ImportIncomeRow {
    public decimal Amount { get; set; }
    public Guid CurrencyId { get; set; }
    public DateTime Date { get; set; }
    public int IncomeStatus { get; set; }
    public string? Description { get; set; }
    public Guid IncomeTypeId { get; set; }
}

public class ImportIncomes(AppDbContext dbContext) : Endpoint<ImportIncomesRequest, ImportIncomesResponse> {
    public override void Configure() {
        Post("incomes/import");
        Version(1);
        Roles(nameof(UserRole.Admin));
        Permissions(nameof(UserPermission.Incomes_Create));
        AllowFileUploads();
        Summary(s => {
            s.Summary = "Imports income records from a CSV file";
            s.Description = "Bulk import income data from a CSV file upload";
        });
    }

    public override async Task HandleAsync(ImportIncomesRequest req, CancellationToken ct) {
        if (req.CsvFile == null || req.CsvFile.Length == 0) {
            AddError(req => req.CsvFile, "CSV file is required.");
            await SendErrorsAsync(400, ct);
            return;
        }

        var result = new ImportIncomesResponse();
        var incomeRecords = new List<ImportIncomeRow>();

        using (var stream = req.CsvFile.OpenReadStream())
        using (var reader = new StreamReader(stream))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture)) {
            try {
                incomeRecords = csv.GetRecords<ImportIncomeRow>().ToList();
            }
            catch (ReaderException ex) {
                AddError($"Failed to parse CSV file: {ex.Message}");
                await SendErrorsAsync(400, ct);
                return;
            }
        }

        if (!incomeRecords.Any()) {
            AddError("The CSV file contains no valid records.");
            await SendErrorsAsync(400, ct);
            return;
        }

        var currencyIds = incomeRecords.Select(r => r.CurrencyId).Distinct().ToList();
        var incomeTypeIds = incomeRecords.Select(r => r.IncomeTypeId).Distinct().ToList();

        var existingCurrencyIds = await dbContext.Currencies
            .Where(c => currencyIds.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync(ct);

        var existingIncomeTypeIds = await dbContext.IncomeTypes
            .Where(it => incomeTypeIds.Contains(it.Id))
            .Select(it => it.Id)
            .ToListAsync(ct);

        foreach (var row in incomeRecords) {
            if (!existingCurrencyIds.Contains(row.CurrencyId)) {
                result.Errors.Add($"Invalid CurrencyId '{row.CurrencyId}' found. Skipped record.");
                continue;
            }

            if (!existingIncomeTypeIds.Contains(row.IncomeTypeId)) {
                result.Errors.Add($"Invalid IncomeTypeId '{row.IncomeTypeId}' found. Skipped record.");
                continue;
            }

            // New validation for integer-based enum
            if (!Enum.IsDefined(typeof(IncomeStatus), row.IncomeStatus)) {
                result.Errors.Add($"Invalid IncomeStatus integer '{row.IncomeStatus}' found. Skipped record.");
                continue;
            }

            var income = new Income {
                Amount = row.Amount,
                Date = row.Date.ToUniversalTime(),
                Description = row.Description,
                CurrencyId = row.CurrencyId,
                IncomeTypeId = row.IncomeTypeId,
                IncomeStatus = (IncomeStatus)row.IncomeStatus // Direct cast from int to enum
            };

            dbContext.Incomes.Add(income);
            result.ImportedCount++;
        }

        await dbContext.SaveChangesAsync(ct);

        await SendAsync(result, 200, ct);
    }
}