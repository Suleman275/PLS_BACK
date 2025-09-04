using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.API.Models;

namespace UserManagement.API.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment> {
    public void Configure(EntityTypeBuilder<Comment> builder) {
        builder.HasQueryFilter(c => c.DeletedOn == null);
    }
}
