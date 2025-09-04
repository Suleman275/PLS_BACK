using SharedKernel.Models;

namespace UserManagement.API.Models;

public class University : EntityBase {
    public string Name { get; set; } = default!;
    public int? NumOfCampuses { get; set; }
    public int? TotalStudents { get; set; }

    public int? YearFounded { get; set; }
    public string? Description { get; set; }

    public Guid UniversityTypeId { get; set; }
    public UniversityType UniversityType { get; set; } = default!;

    public Guid LocationId { get; set; }
    public Location Location { get; set; } = default!;
}