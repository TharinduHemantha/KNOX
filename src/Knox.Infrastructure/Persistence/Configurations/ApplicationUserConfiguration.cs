using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Knox.Infrastructure.Identity;

namespace Knox.Infrastructure.Persistence.Configurations;

internal sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Users");

        builder.Property(x => x.AuthenticationSource)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.DisplayName)
            .HasMaxLength(256);

        builder.Property(x => x.EntraObjectId)
            .HasMaxLength(100);

        builder.Property(x => x.EntraTenantId)
            .HasMaxLength(100);

        builder.Property(x => x.ExternalSubject)
            .HasMaxLength(200);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.IsSoftDeleted)
            .HasDefaultValue(false);

        builder.HasIndex(x => new { x.TenantId, x.Email })
            .HasDatabaseName("IX_Users_TenantId_Email");

        builder.HasIndex(x => new { x.TenantId, x.NormalizedUserName })
            .HasDatabaseName("IX_Users_TenantId_NormalizedUserName");

        builder.HasIndex(x => new { x.TenantId, x.IsSoftDeleted })
            .HasDatabaseName("IX_Users_TenantId_IsSoftDeleted");

        builder.HasIndex(x => new { x.TenantId, x.EntraObjectId })
            .HasDatabaseName("IX_Users_TenantId_EntraObjectId")
            .IsUnique()
            .HasFilter("[EntraObjectId] IS NOT NULL");
    }
}
