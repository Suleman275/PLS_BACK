using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.API.Models;

namespace UserManagement.API.Configurations;

public class DocumentTypeConfiguration : IEntityTypeConfiguration<DocumentType> {
    public void Configure(EntityTypeBuilder<DocumentType> builder) {
        builder.HasQueryFilter(dt => dt.DeletedOn == null);
    }
}