namespace SecureMultiTenant.Domain.Common;

public interface ITenantOwned
{
    Guid TenantId { get; }
}
