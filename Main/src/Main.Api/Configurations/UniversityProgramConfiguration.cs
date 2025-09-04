using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.API.Models;

namespace UserManagement.API.Configurations;

public class UniversityProgramConfiguration : IEntityTypeConfiguration<UniversityProgram> {
    public void Configure(EntityTypeBuilder<UniversityProgram> builder) {
        builder.HasQueryFilter(up => up.DeletedOn == null);
    }
}
