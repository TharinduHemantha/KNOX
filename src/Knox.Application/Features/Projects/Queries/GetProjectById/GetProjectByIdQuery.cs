using Knox.Application.Abstractions.Cqrs;
using Knox.Application.Abstractions.ReadModels;
using Knox.Application.Features.Projects.Common;

namespace Knox.Application.Features.Projects.Queries.GetProjectById;

public sealed record GetProjectByIdQuery(Guid TenantId, Guid ProjectId) : IQuery<ProjectDto?>;

public sealed class GetProjectByIdQueryHandler(IProjectReadRepository projectReadRepository) 
    : IQueryHandler<GetProjectByIdQuery, ProjectDto?>
{
    public Task<ProjectDto?> HandleAsync(GetProjectByIdQuery query, CancellationToken cancellationToken = default)
    {
        return projectReadRepository.GetByIdAsync(query.TenantId, query.ProjectId, cancellationToken);
    }
}
