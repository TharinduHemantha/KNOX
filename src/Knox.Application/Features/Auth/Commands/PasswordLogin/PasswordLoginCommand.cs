using System.Text.Json;
using Knox.Application.Abstractions.Cqrs;
using Knox.Application.Abstractions.Security;
using Knox.Application.Features.Auth.Common;

namespace Knox.Application.Features.Auth.Commands.PasswordLogin;

public sealed record PasswordLoginCommand(
    Guid TenantId,
    string TenantSubdomain,
    string Email,
    string Password) : ICommand<LoginResponse>;

public sealed class PasswordLoginCommandHandler(
    IIdentityService identityService,
    ITokenService tokenService,
    IRefreshTokenService refreshTokenService,
    IAuditService auditService) 
    : ICommandHandler<PasswordLoginCommand, LoginResponse>
{
    public async Task<LoginResponse> HandleAsync(PasswordLoginCommand command, CancellationToken cancellationToken = default)
    {
        var loginResult = await identityService.PasswordLoginAsync(
            command.Email, 
            command.Password, 
            command.TenantId, 
            cancellationToken);

        if (!loginResult.Succeeded || loginResult.UserId is null)
        {
            await auditService.WriteSecurityEventAsync(
                new AuditSecurityEvent(
                    EventType: "Auth.PasswordLogin.Failed",
                    EntityName: "User",
                    EntityId: null,
                    DetailsJson: JsonSerializer.Serialize(new { Email = command.Email, Reason = loginResult.ErrorCode }),
                    TenantId: command.TenantId,
                    UserId: null,
                    CorrelationId: null,
                    IpAddress: null,
                    UserAgent: null),
                cancellationToken);

            throw new UnauthorizedAccessException(loginResult.ErrorDescription ?? "Invalid credentials.");
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
                EventType: "Auth.PasswordLogin.Success",
                EntityName: "User",
                EntityId: loginResult.UserId,
                DetailsJson: JsonSerializer.Serialize(new { Email = command.Email }),
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
