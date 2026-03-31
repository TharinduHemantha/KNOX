using SecureMultiTenant.Application.Abstractions.Cqrs;
using SecureMultiTenant.Application.Abstractions.Persistence;
using SecureMultiTenant.Application.Abstractions.Security;
using SecureMultiTenant.Application.Common.Exceptions;
using SecureMultiTenant.Application.Common.Security;
using SecureMultiTenant.Domain.Entities;
using SecureMultiTenant.Domain.Repositories;
using SecureMultiTenant.Domain.Security;

namespace SecureMultiTenant.Application.Features.Projects.Commands.CreateProject;

public sealed class CreateProjectCommandHandler(
    IProjectRepository projectRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    ICurrentUserService currentUserService)
    : ICommandHandler<CreateProjectCommand, Guid>
{
    public async Task<Guid> HandleAsync(CreateProjectCommand command, CancellationToken cancellationToken = default)
    {
        if (!tenantContext.IsResolved || tenantContext.TenantId is null)
        {
            throw new TenantResolutionException("Tenant was not resolved.");
        }

        AuthorizationGuard.RequirePermission(currentUserService, PermissionNames.ProjectsWrite);

        var existing = await projectRepository.GetByCodeAsync(tenantContext.TenantId.Value, command.Code, cancellationToken);
        if (existing is not null)
        {
            throw new ValidationException("Project code already exists for this tenant.");
        }

        var project = new Project(tenantContext.TenantId.Value, command.Name, command.Code, command.Description);
        project.MarkCreated(currentUserService.UserId);

        await projectRepository.AddAsync(project, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return project.Id;
    }
}
