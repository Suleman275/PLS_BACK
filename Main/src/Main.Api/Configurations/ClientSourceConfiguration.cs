using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.API.Models;

namespace Main.Api.Configurations;

public class ClientSourceConfiguration : IEntityTypeConfiguration<ClientSource> {
    public void Configure(EntityTypeBuilder<ClientSource> builder) {
        builder.HasQueryFilter(cs => cs.DeletedOn == null);
    }
}
