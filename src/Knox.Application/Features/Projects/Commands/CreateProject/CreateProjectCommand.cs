using Knox.Application.Abstractions.Cqrs;
using Knox.Domain.Entities;
using Knox.Domain.Repositories;

namespace Knox.Application.Features.Projects.Commands.CreateProject;

public sealed record CreateProjectCommand(
    Guid TenantId,
    string Name,
    string Code,
    string? Description) : ICommand<Guid>;

public sealed class CreateProjectCommandHandler(IProjectRepository projectRepository) 
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

        return project.Id;
    }
}
