using System.Text.Json;
using Knox.Application.Abstractions.Cqrs;
using Knox.Application.Abstractions.Security;
using Knox.Application.Features.Auth.Common;

namespace Knox.Application.Features.Auth.Commands.EntraExchange;

public sealed record EntraExchangeCommand(
    Guid TenantId,
    string TenantSubdomain,
    string EntraIdToken) : ICommand<LoginResponse>;

public sealed class EntraExchangeCommandHandler(
    IEntraTokenValidator entraTokenValidator,
    IIdentityService identityService,
    ITokenService tokenService,
    IRefreshTokenService refreshTokenService,
    IAuditService auditService) 
    : ICommandHandler<EntraExchangeCommand, LoginResponse>
{
    public async Task<LoginResponse> HandleAsync(EntraExchangeCommand command, CancellationToken cancellationToken = default)
    {
        var validationResult = await entraTokenValidator.ValidateIdTokenAsync(command.EntraIdToken, cancellationToken);

        if (!validationResult.Succeeded || validationResult.Email is null)
        {
            await auditService.WriteSecurityEventAsync(
                new AuditSecurityEvent(
                    EventType: "Auth.EntraExchange.TokenValidationFailed",
                    EntityName: "User",
                    EntityId: null,
                    DetailsJson: JsonSerializer.Serialize(new { Reason = validationResult.ErrorCode }),
                    TenantId: command.TenantId,
                    UserId: null,
                    CorrelationId: null,
                    IpAddress: null,
                    UserAgent: null),
                cancellationToken);

            throw new UnauthorizedAccessException(validationResult.ErrorDescription ?? "Invalid Entra ID token.");
        }

        var loginResult = await identityService.EntraLoginAsync(
            new EntraLoginRequest(
                command.TenantId,
                validationResult.EntraTenantId!,
                validationResult.ObjectId!,
                validationResult.Subject!,
                validationResult.Email,
                validationResult.DisplayName,
                validationResult.IdentityProvider,
                validationResult.PreferredUserName),
            cancellationToken);

        if (!loginResult.Succeeded || loginResult.UserId is null)
        {
            await auditService.WriteSecurityEventAsync(
                new AuditSecurityEvent(
                    EventType: "Auth.EntraExchange.LoginFailed",
                    EntityName: "User",
                    EntityId: null,
                    DetailsJson: JsonSerializer.Serialize(new { Email = validationResult.Email, Reason = loginResult.ErrorCode }),
                    TenantId: command.TenantId,
                    UserId: null,
                    CorrelationId: null,
                    IpAddress: null,
                    UserAgent: null),
                cancellationToken);

            throw new UnauthorizedAccessException(loginResult.ErrorDescription ?? "Entra login failed.");
        }

        var tokenResult = await tokenService.CreateAccessAndRefreshTokensAsync(
            new TokenRequest(
                loginResult.UserId,
                loginResult.Email!,
                command.TenantId,
                command.TenantSubdomain,
                loginResult.Roles ?? [],
                loginResult.Permissions ?? []),
            cancellationToken);

        await refreshTokenService.StoreAsync(
            new StoreRefreshTokenRequest(
                Guid.Parse(loginResult.UserId),
                command.TenantId,
                tokenResult.RefreshTokenHash,
                tokenResult.RefreshTokenExpiresAtUtc,
                null),
            cancellationToken);

        await auditService.WriteSecurityEventAsync(
            new AuditSecurityEvent(
                EventType: "Auth.EntraExchange.Success",
                EntityName: "User",
                EntityId: loginResult.UserId,
                DetailsJson: JsonSerializer.Serialize(new { Email = validationResult.Email, EntraObjectId = validationResult.ObjectId }),
                TenantId: command.TenantId,
                UserId: Guid.Parse(loginResult.UserId),
                CorrelationId: null,
                IpAddress: null,
                UserAgent: null),
            cancellationToken);

        return new LoginResponse(
            tokenResult.AccessToken, 
            tokenResult.RefreshToken, 
            tokenResult.AccessTokenExpiresAtUtc);
    }
}
