using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.API.Models;

namespace UserManagement.API.Configurations;

public class IncomeConfiguration : IEntityTypeConfiguration<Income> {
    public void Configure(EntityTypeBuilder<Income> builder) {
        builder.HasQueryFilter(i => i.DeletedOn == null);

        builder.Property(u => u.IncomeStatus)
               .HasConversion<string>()
               .IsRequired();
    }
}
