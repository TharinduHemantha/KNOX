using Microsoft.EntityFrameworkCore;
using Knox.Domain.Entities;
using Knox.Domain.Repositories;

namespace Knox.Infrastructure.Persistence.Repositories;

public sealed class ProjectRepository(AppDbContext dbContext) : IProjectRepository
{
    public Task<Project?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
        => dbContext.Projects.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id && !x.IsDeleted, cancellationToken);

    public Task<Project?> GetByCodeAsync(Guid tenantId, string code, CancellationToken cancellationToken = default)
        => dbContext.Projects.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, cancellationToken);

    public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
        => await dbContext.Projects.AddAsync(project, cancellationToken);

    public Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        dbContext.Projects.Update(project);
        return Task.CompletedTask;
    }
}
