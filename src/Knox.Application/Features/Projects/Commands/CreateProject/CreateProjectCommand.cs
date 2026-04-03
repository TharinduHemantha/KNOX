using System.Text.Json;
using Knox.Application.Abstractions.Cqrs;
using Knox.Application.Abstractions.Security;
using Knox.Domain.Entities;
using Knox.Domain.Repositories;

namespace Knox.Application.Features.Projects.Commands.CreateProject;

public sealed record CreateProjectCommand(
    Guid TenantId,
    string Name,
    string Code,
    string? Description) : ICommand<Guid>;

public sealed class CreateProjectCommandHandler(
    IProjectRepository projectRepository,
    IAuditService auditService) 
    : ICommandHandler<CreateProjectCommand, Guid>
{
    public async Task<Guid> HandleAsync(CreateProjectCommand command, CancellationToken cancellationToken = default)
    {
        var project = new Project(
            command.TenantId,
            command.Name,
            command.Code,
            command.Description);

        await projectRepository.AddAsync(project, cancellationToken);

        await auditService.WriteSecurityEventAsync(
            new AuditSecurityEvent(
                EventType: "Project.Created",
                EntityName: "Project",
                EntityId: project.Id.ToString(),
                DetailsJson: JsonSerializer.Serialize(new { project.Name, project.Code }),
                TenantId: command.TenantId,
                UserId: null,
                CorrelationId: null,
                IpAddress: null,
                UserAgent: null),
            cancellationToken);

        return project.Id;
    }
}
