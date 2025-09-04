using CsvHelper;
using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using System.Globalization;
using UserManagement.API.Enums;
using UserManagement.API.Models;

public class ImportUniversityRow {
    public string Name { get; set; } = default!;
    public int? NumberOfCampuses { get; set; }
}

public class ImportUniversitiesRequest {
    public IFormFile CsvFile { get; set; } = default!;
}

public class ImportUniversitiesResponse {
    public int ImportedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class ImportUniversities(AppDbContext dbContext) : Endpoint<ImportUniversitiesRequest, ImportUniversitiesResponse> {
    public override void Configure() {
        Post("universities/import");
        Version(1);
        Roles(nameof(UserRole.Admin));
        Permissions(nameof(UserPermission.Universities_Create));
        AllowFileUploads();
        Summary(s => {
            s.Summary = "Imports universities from a CSV file";
            s.Description = "Bulk import universities from a CSV file upload";
        });
    }

    public override async Task HandleAsync(ImportUniversitiesRequest req, CancellationToken ct) {
        if (req.CsvFile == null || req.CsvFile.Length == 0) {
            AddError("CsvFile", "CSV file is required");
            await SendErrorsAsync(400, ct);
            return;
        }

        var result = new ImportUniversitiesResponse {
            ImportedCount = 0,
            Errors = new List<string>()
        };

        var initialLocation = await dbContext.Locations.FirstOrDefaultAsync(cancellationToken: ct);
        var initialType = await dbContext.UniversityTypes.FirstOrDefaultAsync(cancellationToken: ct);

        try {
            // Open and read the CSV file
            using var stream = req.CsvFile.OpenReadStream();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            // Get all records from the CSV file
            var records = csv.GetRecords<ImportUniversityRow>();

            // Process each row one by one
            foreach (var row in records) {
                // Perform basic validation on required fields
                if (string.IsNullOrWhiteSpace(row.Name)) {
                    result.Errors.Add("Skipped row due to missing Name.");
                    continue;
                }

                // Check for duplicate university name
                bool exists = await dbContext.Universities
                    .AnyAsync(u => u.Name == row.Name, ct);

                if (exists) {
                    result.Errors.Add($"University '{row.Name}' already exists. Skipped.");
                    continue;
                }

                // Create a new University model
                var university = new University {
                    Name = row.Name.Trim(),
                    NumOfCampuses = row.NumberOfCampuses,
                    TotalStudents = null,
                    YearFounded = null,
                    Description = null,
                    UniversityTypeId = initialType.Id,
                    LocationId = initialLocation.Id
                };

                // Add the new university to the database context
                dbContext.Universities.Add(university);
                result.ImportedCount++;
            }

            // Save all new universities to the database in a single batch
            await dbContext.SaveChangesAsync(ct);

            // Send a success response with the import details
            await SendAsync(result, 200, ct);
        }
        catch (Exception ex) {
            // Catch any unexpected errors during processing
            AddError("Processing", $"An error occurred while processing the file: {ex.Message}");
            await SendErrorsAsync(500, ct);
        }
    }
}
