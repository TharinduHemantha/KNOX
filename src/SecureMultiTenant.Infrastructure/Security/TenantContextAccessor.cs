using SecureMultiTenant.Application.Abstractions.Security;

namespace SecureMultiTenant.Infrastructure.Security;

public sealed class TenantContextAccessor : ITenantContext
{
    public Guid? TenantId { get; private set; }
    public string? TenantSubdomain { get; private set; }
    public bool IsResolved => TenantId.HasValue && !string.IsNullOrWhiteSpace(TenantSubdomain);

    public void Set(Guid tenantId, string subdomain)
    {
        TenantId = tenantId;
        TenantSubdomain = subdomain;
    }
}
