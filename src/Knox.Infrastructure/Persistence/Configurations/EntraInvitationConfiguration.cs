using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Knox.Infrastructure.Persistence.Entities;

namespace Knox.Infrastructure.Persistence.Configurations;

public sealed class EntraInvitationConfiguration : IEntityTypeConfiguration<EntraInvitation>
{
    public void Configure(EntityTypeBuilder<EntraInvitation> builder)
    {
        builder.ToTable("EntraInvitations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.NormalizedEmail).HasMaxLength(256).IsRequired();
        builder.Property(x => x.EntraTenantId).HasMaxLength(128);
        builder.Property(x => x.RoleName).HasMaxLength(256);
        builder.Property(x => x.InvitationCodeHash).HasMaxLength(128).IsRequired();
        builder.Property(x => x.AcceptedEntraObjectId).HasMaxLength(128);

        builder.HasIndex(x => new { x.TenantId, x.NormalizedEmail, x.IsRevoked, x.AcceptedAtUtc });
        builder.HasIndex(x => x.ExpiresAtUtc);
    }
}
