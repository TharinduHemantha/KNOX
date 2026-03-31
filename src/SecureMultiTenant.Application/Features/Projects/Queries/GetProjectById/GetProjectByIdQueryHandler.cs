using SecureMultiTenant.Application.Abstractions.Cqrs;
using SecureMultiTenant.Application.Abstractions.ReadModels;
using SecureMultiTenant.Application.Abstractions.Security;
using SecureMultiTenant.Application.Common.Exceptions;
using SecureMultiTenant.Application.Common.Security;
using SecureMultiTenant.Application.Features.Projects.Common;
using SecureMultiTenant.Domain.Security;

namespace SecureMultiTenant.Application.Features.Projects.Queries.GetProjectById;

public sealed class GetProjectByIdQueryHandler(
    IProjectReadRepository projectReadRepository,
    ITenantContext tenantContext,
    ICurrentUserService currentUserService)
    : IQueryHandler<GetProjectByIdQuery, ProjectDto?>
{
    public async Task<ProjectDto?> HandleAsync(GetProjectByIdQuery query, CancellationToken cancellationToken = default)
    {
        if (!tenantContext.IsResolved || tenantContext.TenantId is null)
        {
            throw new TenantResolutionException("Tenant was not resolved.");
        }

        AuthorizationGuard.RequirePermission(currentUserService, PermissionNames.ProjectsRead);
        return await projectReadRepository.GetByIdAsync(tenantContext.TenantId.Value, query.Id, cancellationToken);
    }
}
