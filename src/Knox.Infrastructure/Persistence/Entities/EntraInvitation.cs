namespace Knox.Infrastructure.Persistence.Entities;

public sealed class EntraInvitation
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Email { get; set; } = default!;
    public string NormalizedEmail { get; set; } = default!;
    public string? EntraTenantId { get; set; }
    public string? RoleName { get; set; }
    public string InvitationCodeHash { get; set; } = default!;
    public Guid InvitedByUserId { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset? AcceptedAtUtc { get; set; }
    public string? AcceptedEntraObjectId { get; set; }
    public bool IsRevoked { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
