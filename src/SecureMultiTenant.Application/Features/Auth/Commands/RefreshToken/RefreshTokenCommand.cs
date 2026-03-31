using SecureMultiTenant.Application.Abstractions.Cqrs;

namespace SecureMultiTenant.Application.Features.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<LoginResponse>;
