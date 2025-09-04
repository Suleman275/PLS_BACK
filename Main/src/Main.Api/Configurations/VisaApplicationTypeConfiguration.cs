using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.API.Models;

namespace UserManagement.API.Configurations;

public class VisaApplicationTypeConfiguration : IEntityTypeConfiguration<VisaApplicationType> {
    public void Configure(EntityTypeBuilder<VisaApplicationType> builder) {
        builder.HasQueryFilter(vat => vat.DeletedOn == null);
    }
}
