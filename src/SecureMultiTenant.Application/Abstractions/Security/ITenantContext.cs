namespace SecureMultiTenant.Application.Abstractions.Security;

public interface ITenantContext
{
    Guid? TenantId { get; }
    string? TenantSubdomain { get; }
    bool IsResolved { get; }
}
