using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.API.Models;

namespace UserManagement.API.Configurations;

public class IncomeTypeConfiguration : IEntityTypeConfiguration<IncomeType> {
    public void Configure(EntityTypeBuilder<IncomeType> builder) {
        builder.HasQueryFilter(it => it.DeletedOn == null);
    }
}
