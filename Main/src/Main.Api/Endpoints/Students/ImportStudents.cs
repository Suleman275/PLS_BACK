using CsvHelper;
using FastEndpoints;
using Main.Api.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using System.Globalization;
using UserManagement.API.Constants;
using UserManagement.API.Endpoints.Students;
using UserManagement.API.Enums;
using UserManagement.API.Models;

public class ImportStudentsRequest {
    [FromClaim(ClaimNames.SubjectId)]
    public Guid SubjectId { get; set; }
    public IFormFile CsvFile { get; set; } = default!;
}

public class ImportStudents(AppDbContext dbContext) : Endpoint<ImportStudentsRequest, ImportStudentsResponse> {
    public override void Configure() {
        Post("students/import");
        Version(1);
        Roles(nameof(UserRole.Admin));
        Permissions(nameof(UserPermission.Students_Create));
        AllowFileUploads();
    }

    public override async Task HandleAsync(ImportStudentsRequest req, CancellationToken ct) {
        if (req.CsvFile == null || req.CsvFile.Length == 0) {
            AddError("CsvFile", "CSV file is required");
            await SendErrorsAsync(400, ct);
            return;
        }

        var result = new ImportStudentsResponse {
            ImportedCount = 0,
            Errors = new List<string>()
        };

        using var stream = req.CsvFile.OpenReadStream();
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var records = csv.GetRecords<ImportStudentRow>();

        foreach (var row in records) {
            bool exists = await dbContext.Users.AnyAsync(u => u.Email == row.Email, ct);
            if (exists) {
                result.Errors.Add($"Email '{row.Email}' already exists. Skipped.");
                continue;
            }

            var student = new StudentUser {
                Email = row.Email,
                FirstName = row.FirstName,
                MiddleName = row.MiddleName,
                LastName = row.LastName,
                PhoneNumber = row.PhoneNumber,
                DateOfBirth = row.DateOfBirth.Value.ToUniversalTime(),
                CreatedById = req.SubjectId,
                IsActive = true,
                IsEmailVerified = false,
                IsPhoneVerified = false
            };

            foreach (var permission in DefaultPermissionGroups.StudentPermissions) {
                student.Permissions.Add(new PermissionAssignment {
                    Permission = permission
                });
            }

            dbContext.Users.Add(student);
            result.ImportedCount++;
        }

        await dbContext.SaveChangesAsync(ct);

        await SendAsync(result, 200, ct);
    }
}

public class ImportStudentRow {
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
}

public class ImportStudentsResponse {
    public int ImportedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
