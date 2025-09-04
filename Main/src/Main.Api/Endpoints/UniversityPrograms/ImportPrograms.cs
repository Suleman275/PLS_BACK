namespace UserManagement.API.Endpoints.UniversityPrograms;

using CsvHelper;
using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using System.Globalization;
using UserManagement.API.Enums;
using UserManagement.API.Models;

public class ImportProgramRow {
    public string UniversityName { get; set; } = default!;
    public string Name { get; set; } = default!;
    public int? DurationYears { get; set; }
    public string? Description { get; set; }
    //public bool? IsActive { get; set; }
}

public class ImportProgramsRequest {
    public IFormFile CsvFile { get; set; } = default!;
}

public class ImportProgramsResponse {
    public int ImportedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class ImportPrograms(AppDbContext dbContext) : Endpoint<ImportProgramsRequest, ImportProgramsResponse> {
    public override void Configure() {
        Post("programs/import");
        Version(1);
        Roles(nameof(UserRole.Admin));
        Permissions(nameof(UserPermission.UniversityPrograms_Create));
        AllowFileUploads();
        Summary(s => {
            s.Summary = "Imports university programs from a CSV file";
            s.Description = "Bulk import university programs from a CSV file upload";
        });
    }

    public override async Task HandleAsync(ImportProgramsRequest req, CancellationToken ct) {
        if (req.CsvFile == null || req.CsvFile.Length == 0) {
            AddError("CsvFile", "CSV file is required");
            await SendErrorsAsync(400, ct);
            return;
        }

        var result = new ImportProgramsResponse { ImportedCount = 0, Errors = new List<string>() };

        // Assuming a default ProgramType with a name like "General"
        var defaultProgramType = await dbContext.ProgramTypes.FirstOrDefaultAsync(pt => pt.Name == "Other", ct);

        if (defaultProgramType == null) {
            AddError("ProgramType", "No default program type exists in the database. Please create one.");
            await SendErrorsAsync(500, ct);
            return;
        }

        try {
            using var stream = req.CsvFile.OpenReadStream();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<ImportProgramRow>().ToList(); // Use ToList() to avoid issues with multiple enumerations

            foreach (var row in records) {
                if (string.IsNullOrWhiteSpace(row.Name) || string.IsNullOrWhiteSpace(row.UniversityName)) {
                    result.Errors.Add("Skipped row due to missing Program Name or University Name.");
                    continue;
                }

                // Find the University by name
                var university = await dbContext.Universities
                    .FirstOrDefaultAsync(u => u.Name.Trim() == row.UniversityName.Trim(), ct);

                if (university == null) {
                    result.Errors.Add($"University '{row.UniversityName}' not found. Skipped program '{row.Name}'.");
                    continue;
                }

                // Check for duplicate program name within the same university
                bool programExists = await dbContext.UniversityPrograms
                    .AnyAsync(p => p.Name == row.Name && p.UniversityId == university.Id, ct);

                if (programExists) {
                    result.Errors.Add($"Program '{row.Name}' already exists for university '{row.UniversityName}'. Skipped.");
                    continue;
                }

                // Create a new UniversityProgram model
                var program = new UniversityProgram {
                    Name = row.Name.Trim(),
                    DurationYears = row.DurationYears ?? 4, // Default to 4 years if not provided
                    Description = row.Description?.Trim(),
                    IsActive = true, // Default to true if not provided
                    UniversityId = university.Id,
                    ProgramTypeId = defaultProgramType.Id
                };

                dbContext.UniversityPrograms.Add(program);
                result.ImportedCount++;
            }

            await dbContext.SaveChangesAsync(ct);

            await SendAsync(result, 200, ct);
        }
        catch (CsvHelperException ex) {
            AddError("FileFormat", $"CSV format error on line {ex.Context.Parser.Row}: {ex.Message}. Check column headers and data types.");
            await SendErrorsAsync(400, ct);
        }
        catch (Exception ex) {
            AddError("Processing", $"An unexpected error occurred: {ex.Message}");
            await SendErrorsAsync(500, ct);
        }
    }
}