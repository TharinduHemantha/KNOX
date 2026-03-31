using SecureMultiTenant.Application.Abstractions.Cqrs;
using SecureMultiTenant.Application.Features.Auth.Commands.PasswordLogin;

namespace SecureMultiTenant.Application.Features.Auth.Commands.EntraExchange;

public sealed record EntraExchangeCommand(string IdToken) : ICommand<LoginResponse>;
