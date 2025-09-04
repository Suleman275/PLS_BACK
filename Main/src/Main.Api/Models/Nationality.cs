using SharedKernel.Models;

namespace UserManagement.API.Models;

public class Nationality : EntityBase {
    public string Name { get; set; } = default!;
    public string? TwoLetterCode { get; set; }
    public string? ThreeLetterCode { get; set; }
}