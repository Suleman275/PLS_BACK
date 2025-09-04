using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.API.Models;

namespace UserManagement.API.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense> {
    public void Configure(EntityTypeBuilder<Expense> builder) {
        builder.HasQueryFilter(e => e.DeletedOn == null);
    }
}
