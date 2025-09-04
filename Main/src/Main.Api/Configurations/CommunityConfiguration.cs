using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.API.Models;

namespace UserManagement.API.Configurations;

public class CommunityConfiguration : IEntityTypeConfiguration<Community> {
    public void Configure(EntityTypeBuilder<Community> builder) {
        builder.HasQueryFilter(c => c.DeletedOn == null);
    }
}

