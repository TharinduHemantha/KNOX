using Microsoft.AspNetCore.Identity;

namespace SecureMultiTenant.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public Guid TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsSoftDeleted { get; set; }
    public string AuthenticationSource { get; set; } = "Local";
    public string? DisplayName { get; set; }
    public string? EntraObjectId { get; set; }
    public string? EntraTenantId { get; set; }
    public string? ExternalSubject { get; set; }
    public DateTimeOffset? LastLoginAtUtc { get; set; }
}
