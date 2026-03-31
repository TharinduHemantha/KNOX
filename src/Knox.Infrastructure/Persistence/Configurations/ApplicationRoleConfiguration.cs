using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Knox.Infrastructure.Identity;

namespace Knox.Infrastructure.Persistence.Configurations;

internal sealed class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.ToTable("Roles");

        builder.Property(x => x.IsSystemRole)
            .HasDefaultValue(false);

        builder.HasIndex(x => new { x.TenantId, x.Name })
            .HasDatabaseName("IX_Roles_TenantId_Name");
    }
}
