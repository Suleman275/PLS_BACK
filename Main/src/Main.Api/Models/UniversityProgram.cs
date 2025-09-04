using SharedKernel.Models;

namespace UserManagement.API.Models;

public class UniversityProgram : EntityBase {
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int DurationYears { get; set; }
    public bool IsActive { get; set; }

    public Guid UniversityId { get; set; }
    public University University { get; set; } = default!;

    public Guid ProgramTypeId { get; set; }
    public ProgramType ProgramType { get; set; } = default!;
}