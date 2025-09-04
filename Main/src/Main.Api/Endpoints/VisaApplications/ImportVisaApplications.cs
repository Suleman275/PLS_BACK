using CsvHelper;
using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Enums;
using System.Globalization;
using UserManagement.API.Enums;
using UserManagement.API.Models;

// Data Transfer Object for a single row in the CSV file
public class ImportVisaApplicationRow {
    public string Email { get; set; } = default!;
    public int? Status { get; set; }
    //public string VisaTypeName { get; set; } = default!;
}

// Request model for the API endpoint
public class ImportVisaApplicationsRequest {
    public IFormFile CsvFile { get; set; } = default!;
}

// Response model to provide feedback to the client
public class ImportVisaApplicationsResponse {
    public int ImportedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}

// FastEndpoints endpoint for importing visa applications
public class ImportVisaApplications(AppDbContext dbContext) : Endpoint<ImportVisaApplicationsRequest, ImportVisaApplicationsResponse> {
    public override void Configure() {
        Post("visa-applications/import");
        Version(1);
        Roles(nameof(UserRole.Admin));
        Permissions(nameof(UserPermission.UniversityApplications_Create));
        AllowFileUploads();
        Summary(s => {
            s.Summary = "Imports visa applications from a CSV file";
            s.Description = "Bulk import visa applications from a CSV file upload";
        });
    }

    public override async Task HandleAsync(ImportVisaApplicationsRequest req, CancellationToken ct) {
        if (req.CsvFile == null || req.CsvFile.Length == 0) {
            AddError("CsvFile", "CSV file is required.");
            await SendErrorsAsync(400, ct);
            return;
        }

        var result = new ImportVisaApplicationsResponse { ImportedCount = 0, Errors = new List<string>() };

        try {
            using var stream = req.CsvFile.OpenReadStream();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<ImportVisaApplicationRow>().ToList();

            foreach (var row in records) {
                // Validate required fields from the CSV row
                if (string.IsNullOrWhiteSpace(row.Email) || row.Status == null) {
                    result.Errors.Add("Skipped row due to missing Email, Visa Type Name, or Status.");
                    continue;
                }

                // Find the applicant by email
                var applicant = await dbContext.Users
                    .FirstOrDefaultAsync(u => u.Email.Trim() == row.Email.Trim(), ct);

                if (applicant == null) {
                    result.Errors.Add($"Applicant with email '{row.Email}' not found. Skipped application for visa type '.");
                    continue;
                }

                // Find the visa application type by name
                var visaType = await dbContext.VisaApplicationTypes
                    .FirstOrDefaultAsync(v => v.Name.Trim() == "Other", ct);

                //if (visaType == null) {
                //    result.Errors.Add($"Visa application type '{row.VisaTypeName}' not found. Skipped application for applicant '{row.Email}'.");
                //    continue;
                //}

                // Check for duplicate applications
                bool applicationExists = await dbContext.VisaApplications
                    .AnyAsync(a => a.ApplicantId == applicant.Id && a.VisaApplicationTypeId == visaType.Id, ct);

                if (applicationExists) {
                    result.Errors.Add($"Application already exists for '{row.Email}' to visa type ''. Skipped.");
                    continue;
                }

                // Parse the status and apply the specific business logic (map 3 to 0)
                var applicationStatus = ApplicationStatus.UnderReview;
                if (Enum.IsDefined(typeof(ApplicationStatus), row.Status)) {
                    applicationStatus = (ApplicationStatus)row.Status;
                }

                // Specific business rule: if the CSV status is 3, map it to UnderReview (0)
                if (row.Status == 3) {
                    applicationStatus = ApplicationStatus.UnderReview;
                }

                // Create a new VisaApplication entity
                var application = new VisaApplication {
                    ApplicantId = applicant.Id,
                    VisaApplicationTypeId = visaType.Id,
                    ApplicationStatus = applicationStatus,
                    ApplyDate = DateTime.UtcNow,
                    SubmissionDate = DateTime.UtcNow,
                };

                dbContext.VisaApplications.Add(application);
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
