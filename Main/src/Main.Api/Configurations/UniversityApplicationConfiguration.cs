using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.API.Models;

namespace UserManagement.API.Configurations {
    public class UniversityApplicationConfiguration : IEntityTypeConfiguration<UniversityApplication> {
        public void Configure(EntityTypeBuilder<UniversityApplication> builder) {
            builder.HasQueryFilter(ua => ua.DeletedOn == null);
        }
    }
}
