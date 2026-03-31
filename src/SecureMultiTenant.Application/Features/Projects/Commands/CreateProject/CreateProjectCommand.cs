using SecureMultiTenant.Application.Abstractions.Cqrs;

namespace SecureMultiTenant.Application.Features.Projects.Commands.CreateProject;

public sealed record CreateProjectCommand(string Name, string Code, string? Description) : ICommand<Guid>;
