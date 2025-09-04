using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UserManagement.API.Configurations {
    public class ExpenseTypeConfiguration : IEntityTypeConfiguration<ExpenseType> {
        public void Configure(EntityTypeBuilder<ExpenseType> builder) {
            builder.HasQueryFilter(et => et.DeletedOn == null);
        }
    }
}
