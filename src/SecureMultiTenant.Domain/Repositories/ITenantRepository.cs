using SecureMultiTenant.Domain.Entities;

namespace SecureMultiTenant.Domain.Repositories;

public interface ITenantRepository
{
    Task<Tenant?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default);
    Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
