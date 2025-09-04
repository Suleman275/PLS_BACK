using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.API.Models;

namespace UserManagement.API.Configurations {
    public class LocationConfiguration : IEntityTypeConfiguration<Location> {
        public void Configure(EntityTypeBuilder<Location> builder) {
            builder.HasQueryFilter(l => l.DeletedOn == null);
        }
    }
}
