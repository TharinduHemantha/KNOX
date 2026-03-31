using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Knox.Application.Abstractions.Cqrs;
using Knox.Application.Features.Auth.Commands.CreateEntraInvitation;
using Knox.Application.Features.Auth.Commands.EntraExchange;
using Knox.Application.Features.Auth.Commands.PasswordLogin;
using Knox.Application.Features.Auth.Commands.RefreshToken;
using Knox.Application.Features.Auth.Commands.RevokeRefreshToken;
using Knox.Application.Features.Auth.Common;
using Knox.Application.Features.Projects.Commands.CreateProject;
using Knox.Application.Features.Projects.Common;
using Knox.Application.Features.Projects.Queries.GetProjectById;
using Knox.Application.Infrastructure;

namespace Knox.Application;

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
        services.AddScoped<IQueryHandler<GetProjectByIdQuery, ProjectDto?>, GetProjectByIdQueryHandler>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
