using SecureMultiTenant.Application.Common.Exceptions;
using SecureMultiTenant.Domain.Repositories;
using SecureMultiTenant.Infrastructure.Security;

namespace SecureMultiTenant.Api.Middleware;

public sealed class TenantResolutionMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context, ITenantRepository tenantRepository, TenantContextAccessor tenantContextAccessor)
    {
        var host = context.Request.Host.Host;

        if (string.IsNullOrWhiteSpace(host) || host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        var parts = host.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3)
        {
            throw new TenantResolutionException("Tenant subdomain is missing.");
        }

        var subdomain = parts[0].ToLowerInvariant();
        var tenant = await tenantRepository.GetBySubdomainAsync(subdomain, context.RequestAborted);
        if (tenant is null)
        {
            throw new TenantResolutionException("Tenant could not be resolved.");
        }

        tenantContextAccessor.Set(tenant.Id, tenant.Subdomain);
        await next(context);
    }
}
