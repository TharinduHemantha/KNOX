using SecureMultiTenant.Domain.Entities;

namespace SecureMultiTenant.Domain.Repositories;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default);
    Task<Project?> GetByCodeAsync(Guid tenantId, string code, CancellationToken cancellationToken = default);
    Task AddAsync(Project project, CancellationToken cancellationToken = default);
    Task UpdateAsync(Project project, CancellationToken cancellationToken = default);
}
