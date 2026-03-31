using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecureMultiTenant.Infrastructure.Identity;
using SecureMultiTenant.Infrastructure.Persistence.Entities;

namespace SecureMultiTenant.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.TokenHash)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(x => x.ReplacedByTokenHash)
            .HasMaxLength(512);

        builder.Property(x => x.RevokedBy)
            .HasMaxLength(256);

        builder.Property(x => x.CreatedByIp)
            .HasMaxLength(64);

        builder.HasIndex(x => x.TokenHash)
            .IsUnique()
            .HasDatabaseName("UX_RefreshTokens_TokenHash");

        builder.HasIndex(x => new { x.UserId, x.TenantId, x.ExpiresAtUtc })
            .HasDatabaseName("IX_RefreshTokens_UserId_TenantId_ExpiresAtUtc");

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_RefreshTokens_Users_UserId");

        builder.HasOne<SecureMultiTenant.Domain.Entities.Tenant>()
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_RefreshTokens_Tenants_TenantId");
    }
}
