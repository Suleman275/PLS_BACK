using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.API.Models;

namespace UserManagement.API.Configurations {
    public class VisaApplicationConfiguration : IEntityTypeConfiguration<VisaApplication> {
        public void Configure(EntityTypeBuilder<VisaApplication> builder) {
            builder.HasQueryFilter(va => va.DeletedOn == null);
        }
    }
}
