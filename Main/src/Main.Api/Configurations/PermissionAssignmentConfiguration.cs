using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.API.Models;

namespace UserManagement.API.Configurations;

public class PermissionAssignmentConfiguration : IEntityTypeConfiguration<PermissionAssignment> {
    public void Configure(EntityTypeBuilder<PermissionAssignment> builder) {
        builder.HasQueryFilter(pa => pa.DeletedOn == null);

        builder.Property(pa => pa.Permission)
            .HasConversion<string>()
            .IsRequired();
    }
}
