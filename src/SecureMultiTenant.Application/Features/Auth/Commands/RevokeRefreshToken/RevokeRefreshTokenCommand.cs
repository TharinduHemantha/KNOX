using SecureMultiTenant.Application.Abstractions.Cqrs;

namespace SecureMultiTenant.Application.Features.Auth.Commands.RevokeRefreshToken;

public sealed record RevokeRefreshTokenCommand(string RefreshToken) : ICommand;
