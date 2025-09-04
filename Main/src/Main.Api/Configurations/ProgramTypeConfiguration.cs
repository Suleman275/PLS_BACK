using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.API.Models;

namespace UserManagement.API.Configurations;

public class ProgramTypeConfiguration : IEntityTypeConfiguration<ProgramType> {
    public void Configure(EntityTypeBuilder<ProgramType> builder) {
        builder.HasQueryFilter(pt => pt.DeletedOn == null);
    }
}