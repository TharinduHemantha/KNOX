using SecureMultiTenant.Application.Abstractions.Cqrs;

namespace SecureMultiTenant.Application.Features.Auth.Commands.PasswordLogin;

public sealed record PasswordLoginCommand(string UserNameOrEmail, string Password) : ICommand<LoginResponse>;
public sealed record LoginResponse(string AccessToken, DateTimeOffset AccessTokenExpiresAtUtc, string RefreshToken, DateTimeOffset RefreshTokenExpiresAtUtc);
