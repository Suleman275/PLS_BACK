using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.API.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User> {
    public void Configure(EntityTypeBuilder<User> builder) {
        builder.HasDiscriminator<UserRole>("Role")
               .HasValue<AdminUser>(UserRole.Admin)
               .HasValue<StudentUser>(UserRole.Student)
               .HasValue<EmployeeUser>(UserRole.Employee)
               .HasValue<PartnerUser>(UserRole.Partner)
               .HasValue<ImmigrationClientUser>(UserRole.ImmigrationClient);

        builder.Property(u => u.Role)
               .HasConversion<string>()
               .IsRequired();

        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(100);

        builder.HasQueryFilter(u => u.DeletedOn == null);
    }
}
