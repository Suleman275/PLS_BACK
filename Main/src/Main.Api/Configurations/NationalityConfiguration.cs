using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.API.Models;

namespace UserManagement.API.Configurations {
    public class NationalityConfiguration : IEntityTypeConfiguration<Nationality> {
        public void Configure(EntityTypeBuilder<Nationality> builder) {
            builder.HasQueryFilter(n => n.DeletedOn == null);
        }
    }
}
