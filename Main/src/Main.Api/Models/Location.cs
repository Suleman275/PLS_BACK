using SharedKernel.Models;

namespace UserManagement.API.Models;

public class Location : EntityBase {
    public string City { get; set; } = default!;
    public string Country { get; set; } = default!;
}