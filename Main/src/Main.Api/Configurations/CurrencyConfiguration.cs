using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.API.Models;

namespace UserManagement.API.Configurations {
    public class CurrencyConfiguration : IEntityTypeConfiguration<Currency> {
        public void Configure(EntityTypeBuilder<Currency> builder) {
            builder.HasQueryFilter(c => c.DeletedOn == null);
        }
    }
}
