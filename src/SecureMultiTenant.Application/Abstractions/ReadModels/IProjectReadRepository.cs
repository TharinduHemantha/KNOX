using SecureMultiTenant.Application.Features.Projects.Common;

namespace SecureMultiTenant.Application.Abstractions.ReadModels;

public interface IProjectReadRepository
{
    Task<ProjectDto?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<ProjectListItemDto>> SearchAsync(Guid tenantId, ProjectSearchRequest request, CancellationToken cancellationToken = default);
}
