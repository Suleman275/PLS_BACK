using CsvHelper;
using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using System.Globalization;
using UserManagement.API.Models;

public class ImportLocationRow {
    public string City { get; set; } = default!;
    public string Country { get; set; } = default!;
}

public class ImportLocationsRequest {
    public IFormFile CsvFile { get; set; } = default!;
}

public class ImportLocationsResponse {
    public int ImportedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}


public class ImportLocations(AppDbContext dbContext) : Endpoint<ImportLocationsRequest, ImportLocationsResponse> {
    public override void Configure() {
        Post("locations/import");
        Version(1);
        Permissions(nameof(UserPermission.Locations_Create));
        AllowFileUploads();
    }

    public override async Task HandleAsync(ImportLocationsRequest req, CancellationToken ct) {
        if (req.CsvFile == null || req.CsvFile.Length == 0) {
            AddError("CsvFile", "CSV file is required");
            await SendErrorsAsync(400, ct);
            return;
        }

        var result = new ImportLocationsResponse {
            ImportedCount = 0,
            Errors = new List<string>()
        };

        try {
            using var stream = req.CsvFile.OpenReadStream();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<ImportLocationRow>();

            foreach (var row in records) {
                bool exists = await dbContext.Locations
                    .AnyAsync(l => l.City == row.City && l.Country == row.Country, ct);

                if (exists) {
                    result.Errors.Add($"Location '{row.City}, {row.Country}' already exists. Skipped.");
                    continue;
                }

                var location = new Location {
                    City = row.City,
                    Country = row.Country
                };

                dbContext.Locations.Add(location);
                result.ImportedCount++;
            }

            await dbContext.SaveChangesAsync(ct);

            await SendAsync(result, 200, ct);
        }
        catch (Exception ex) {
            AddError("Processing", $"An error occurred while processing the file: {ex.Message}");
            await SendErrorsAsync(500, ct);
        }
    }
}
