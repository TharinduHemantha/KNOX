using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SecureMultiTenant.Application.Abstractions.Cqrs;
using SecureMultiTenant.Application.Features.Auth.Commands.CreateEntraInvitation;
using SecureMultiTenant.Application.Features.Auth.Commands.EntraExchange;
using SecureMultiTenant.Application.Features.Auth.Commands.PasswordLogin;
using SecureMultiTenant.Application.Features.Auth.Commands.RefreshToken;
using SecureMultiTenant.Application.Features.Auth.Commands.RevokeRefreshToken;
using SecureMultiTenant.Application.Features.Projects.Commands.CreateProject;
using SecureMultiTenant.Application.Features.Projects.Queries.GetProjectById;
using SecureMultiTenant.Application.Infrastructure;

namespace SecureMultiTenant.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();

        services.AddScoped<ICommandHandler<CreateProjectCommand, Guid>, CreateProjectCommandHandler>();
        services.AddScoped<ICommandHandler<CreateEntraInvitationCommand, CreateEntraInvitationResponse>, CreateEntraInvitationCommandHandler>();
        services.AddScoped<ICommandHandler<PasswordLoginCommand, LoginResponse>, PasswordLoginCommandHandler>();
        services.AddScoped<ICommandHandler<RefreshTokenCommand, LoginResponse>, RefreshTokenCommandHandler>();
        services.AddScoped<ICommandHandler<EntraExchangeCommand, LoginResponse>, EntraExchangeCommandHandler>();
        services.AddScoped<ICommandHandler<RevokeRefreshTokenCommand, Unit>, RevokeRefreshTokenCommandHandler>();
        services.AddScoped<IQueryHandler<GetProjectByIdQuery, Features.Projects.Common.ProjectDto?>, GetProjectByIdQueryHandler>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
