using SecureMultiTenant.Application.Abstractions.Cqrs;
using SecureMultiTenant.Application.Features.Auth.Commands.PasswordLogin;

namespace SecureMultiTenant.Application.Features.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<LoginResponse>;
