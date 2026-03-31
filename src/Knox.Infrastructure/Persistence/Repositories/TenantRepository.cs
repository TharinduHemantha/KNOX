using Microsoft.EntityFrameworkCore;
using Knox.Domain.Entities;
using Knox.Domain.Repositories;

namespace Knox.Infrastructure.Persistence.Repositories;

public sealed class TenantRepository(AppDbContext dbContext) : ITenantRepository
{
    public Task<Tenant?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default)
        => dbContext.Tenants.FirstOrDefaultAsync(x => x.Subdomain == subdomain && x.IsActive && !x.IsDeleted, cancellationToken);

    public Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => dbContext.Tenants.FirstOrDefaultAsync(x => x.Id == tenantId && x.IsActive && !x.IsDeleted, cancellationToken);
}
