using CsvHelper;
using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using System.Globalization;
using UserManagement.API.Enums;
using UserManagement.API.Models;

// Request DTO for the new endpoint
public class ImportExpensesRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    public IFormFile CsvFile { get; set; } = default!;
}

// Response DTO for import result
public class ImportExpensesResponse {
    public int ImportedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}

// CSV row model matching your CSV file columns
public class ImportExpenseRow {
    public decimal Amount { get; set; }
    public Guid CurrencyId { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public Guid ExpenseTypeId { get; set; }
}

public class ImportExpenses(AppDbContext dbContext) : Endpoint<ImportExpensesRequest, ImportExpensesResponse> {
    public override void Configure() {
        Post("expenses/import");
        Version(1);
        Roles(nameof(UserRole.Admin));
        Permissions(nameof(UserPermission.Expenses_Create));
        AllowFileUploads();
        Summary(s => {
            s.Summary = "Imports expense records from a CSV file";
            s.Description = "Bulk import expense data from a CSV file upload";
        });
    }

    public override async Task HandleAsync(ImportExpensesRequest req, CancellationToken ct) {
        if (req.CsvFile == null || req.CsvFile.Length == 0) {
            AddError(req => req.CsvFile, "CSV file is required.");
            await SendErrorsAsync(400, ct);
            return;
        }

        var result = new ImportExpensesResponse();
        var expenseRecords = new List<ImportExpenseRow>();

        using (var stream = req.CsvFile.OpenReadStream())
        using (var reader = new StreamReader(stream))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture)) {
            try {
                expenseRecords = csv.GetRecords<ImportExpenseRow>().ToList();
            }
            catch (ReaderException ex) {
                AddError($"Failed to parse CSV file: {ex.Message}");
                await SendErrorsAsync(400, ct);
                return;
            }
        }

        if (!expenseRecords.Any()) {
            AddError("The CSV file contains no valid records.");
            await SendErrorsAsync(400, ct);
            return;
        }

        // Extract unique IDs for validation
        var currencyIds = expenseRecords.Select(r => r.CurrencyId).Distinct().ToList();
        var expenseTypeIds = expenseRecords.Select(r => r.ExpenseTypeId).Distinct().ToList();

        // Validate existence of all IDs in single batch queries
        var existingCurrencyIds = await dbContext.Currencies
            .Where(c => currencyIds.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync(ct);

        var existingExpenseTypeIds = await dbContext.ExpenseTypes
            .Where(et => expenseTypeIds.Contains(et.Id))
            .Select(et => et.Id)
            .ToListAsync(ct);

        foreach (var row in expenseRecords) {
            // Validate CurrencyId
            if (!existingCurrencyIds.Contains(row.CurrencyId)) {
                result.Errors.Add($"Invalid CurrencyId '{row.CurrencyId}' found. Skipped record.");
                continue;
            }

            // Validate ExpenseTypeId
            if (!existingExpenseTypeIds.Contains(row.ExpenseTypeId)) {
                result.Errors.Add($"Invalid ExpenseTypeId '{row.ExpenseTypeId}' found. Skipped record.");
                continue;
            }

            var expense = new Expense {
                Amount = row.Amount,
                Date = row.Date.ToUniversalTime(),
                Description = row.Description,
                CurrencyId = row.CurrencyId,
                ExpenseTypeId = row.ExpenseTypeId,
            };

            dbContext.Expenses.Add(expense);
            result.ImportedCount++;
        }

        await dbContext.SaveChangesAsync(ct);

        await SendAsync(result, 200, ct);
    }
}