namespace UserManagement.API.Endpoints.UniversityApplications;

using CsvHelper;
using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using System.Globalization;
using UserManagement.API.Enums;
using UserManagement.API.Models;

// Data Transfer Object for a single row in the CSV file
public class ImportApplicationRow {
    public string Email { get; set; } = default!;
    public int? Status { get; set; }
    public string Name { get; set; } = default!;
}

// Request model for the API endpoint
public class ImportApplicationsRequest {
    public IFormFile CsvFile { get; set; } = default!;
}

// Response model to provide feedback to the client
public class ImportApplicationsResponse {
    public int ImportedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}

// FastEndpoints endpoint for importing university applications
public class ImportApplications(AppDbContext dbContext) : Endpoint<ImportApplicationsRequest, ImportApplicationsResponse> {
    public override void Configure() {
        Post("applications/import");
        Version(1);
        Roles(nameof(UserRole.Admin));
        Permissions(nameof(UserPermission.UniversityApplications_Create));
        AllowFileUploads();
        Summary(s => {
            s.Summary = "Imports university applications from a CSV file";
            s.Description = "Bulk import university applications from a CSV file upload";
        });
    }

    public override async Task HandleAsync(ImportApplicationsRequest req, CancellationToken ct) {
        if (req.CsvFile == null || req.CsvFile.Length == 0) {
            AddError("CsvFile", "CSV file is required.");
            await SendErrorsAsync(400, ct);
            return;
        }

        var result = new ImportApplicationsResponse { ImportedCount = 0, Errors = new List<string>() };

        try {
            using var stream = req.CsvFile.OpenReadStream();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<ImportApplicationRow>().ToList();

            foreach (var row in records) {
                // Validate required fields from the CSV row
                if (string.IsNullOrWhiteSpace(row.Email) || string.IsNullOrWhiteSpace(row.Name) || row.Status == null) {
                    result.Errors.Add("Skipped row due to missing Email, Program Name, or Status.");
                    continue;
                }

                // Find the applicant by email
                var applicant = await dbContext.Users
                    .FirstOrDefaultAsync(u => u.Email.Trim() == row.Email.Trim(), ct);

                if (applicant == null) {
                    result.Errors.Add($"Applicant with email '{row.Email}' not found. Skipped application for program '{row.Name}'.");
                    continue;
                }

                // Find the university program by name
                var program = await dbContext.UniversityPrograms
                    .FirstOrDefaultAsync(p => p.Name.Trim() == row.Name.Trim(), ct);

                if (program == null) {
                    result.Errors.Add($"University program '{row.Name}' not found. Skipped application for applicant '{row.Email}'.");
                    continue;
                }

                // Check for duplicate applications to prevent multiple applications for the same program from the same applicant
                bool applicationExists = await dbContext.UniversityApplications
                    .AnyAsync(a => a.ApplicantId == applicant.Id && a.UniversityProgramId == program.Id, ct);

                if (applicationExists) {
                    result.Errors.Add($"Application already exists for '{row.Email}' to program '{row.Name}'. Skipped.");
                    continue;
                }

                // Parse the status and apply the specific business logic (map 1 to 0)
                var applicationStatus = ApplicationStatus.UnderReview;
                if (Enum.IsDefined(typeof(ApplicationStatus), row.Status)) {
                    applicationStatus = (ApplicationStatus)row.Status;
                }

                // Specific business rule: if the CSV status is 1, map it to UnderReview (0)
                if (row.Status == 1) {
                    applicationStatus = ApplicationStatus.UnderReview;
                }

                // Create a new UniversityApplication entity
                var application = new UniversityApplication {
                    ApplicantId = applicant.Id,
                    UniversityProgramId = program.Id,
                    ApplicationStatus = applicationStatus,
                    ApplyDate = DateTime.UtcNow,
                    SubmissionDate = DateTime.UtcNow,
                };

                dbContext.UniversityApplications.Add(application);
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
