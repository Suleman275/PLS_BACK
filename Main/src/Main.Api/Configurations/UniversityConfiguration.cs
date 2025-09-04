using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.API.Models;

namespace UserManagement.API.Configurations {
    public class UniversityConfiguration : IEntityTypeConfiguration<University> {
        public void Configure(EntityTypeBuilder<University> builder) {
            builder.HasQueryFilter(u => u.DeletedOn == null);
        }
    }
}
