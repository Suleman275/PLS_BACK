using SharedKernel.Models;

namespace UserManagement.API.Models {
    public class Currency : EntityBase {
        public string Name { get; set; } = default!;
        public string Code { get; set; } = default!;
        public string? Description { get; set; }
    }
}
