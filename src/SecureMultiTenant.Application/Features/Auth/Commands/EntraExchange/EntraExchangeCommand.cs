using SecureMultiTenant.Application.Abstractions.Cqrs;

namespace SecureMultiTenant.Application.Features.Auth.Commands.EntraExchange;

public sealed record EntraExchangeCommand(string IdToken) : ICommand<LoginResponse>;
