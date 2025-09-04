using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.API.Models;

namespace UserManagement.API.Configurations {
    public class DocumentConfiguration : IEntityTypeConfiguration<Document> {
        public void Configure(EntityTypeBuilder<Document> builder) {
            builder.HasQueryFilter(d => d.DeletedOn == null);
        }
    }
}
