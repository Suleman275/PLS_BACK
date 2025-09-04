using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.API.Models;

namespace UserManagement.API.Configurations;

public class UniversityTypeConfiguration : IEntityTypeConfiguration<UniversityType> {
    public void Configure(EntityTypeBuilder<UniversityType> builder) {
        builder.HasQueryFilter(ut => ut.DeletedOn == null);
    }
}
