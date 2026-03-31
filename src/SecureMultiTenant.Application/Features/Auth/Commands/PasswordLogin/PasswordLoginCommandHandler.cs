using System.Text.Json;
using SecureMultiTenant.Application.Abstractions.Cqrs;
using SecureMultiTenant.Application.Abstractions.Security;
using SecureMultiTenant.Application.Common.Exceptions;

namespace SecureMultiTenant.Application.Features.Auth.Commands.PasswordLogin;

public sealed class PasswordLoginCommandHandler(
    IIdentityService identityService,
    ITokenService tokenService,
    IRefreshTokenService refreshTokenService,
    IAuditService auditService,
    ITenantContext tenantContext)
    : ICommandHandler<PasswordLoginCommand, LoginResponse>
{
    public async Task<LoginResponse> HandleAsync(PasswordLoginCommand command, CancellationToken cancellationToken = default)
    {
        if (!tenantContext.IsResolved || tenantContext.TenantId is null || string.IsNullOrWhiteSpace(tenantContext.TenantSubdomain))
        {
            throw new TenantResolutionException("Tenant was not resolved.");
        }

        var loginResult = await identityService.PasswordLoginAsync(
            command.UserNameOrEmail,
            command.Password,
            tenantContext.TenantId.Value,
            cancellationToken);

        if (!loginResult.Succeeded || loginResult.UserId is null || loginResult.Email is null || loginResult.Roles is null || loginResult.Permissions is null)
        {
            await auditService.WriteSecurityEventAsync(
                new AuditSecurityEvent(
                    "auth.local.failed",
                    "ApplicationUser",
                    null,
                    JsonSerializer.Serialize(new { command.UserNameOrEmail }),
                    tenantContext.TenantId,
                    null,
                    null,
                    null,
                    null),
                cancellationToken);

            throw new ForbiddenException("Invalid login request.");
        }

        var tokenResult = await tokenService.CreateAccessAndRefreshTokensAsync(
            new TokenRequest(
                loginResult.UserId,
                loginResult.Email,
                tenantContext.TenantId.Value,
                tenantContext.TenantSubdomain,
                loginResult.Roles,
                loginResult.Permissions),
            cancellationToken);

        await refreshTokenService.StoreAsync(
            new StoreRefreshTokenRequest(
                Guid.Parse(loginResult.UserId),
                tenantContext.TenantId.Value,
                tokenResult.RefreshTokenHash,
                tokenResult.RefreshTokenExpiresAtUtc,
                null),
            cancellationToken);

        await auditService.WriteSecurityEventAsync(
            new AuditSecurityEvent(
                "auth.local.succeeded",
                "ApplicationUser",
                loginResult.UserId,
                JsonSerializer.Serialize(new { tenantId = tenantContext.TenantId, tenantSubdomain = tenantContext.TenantSubdomain }),
                tenantContext.TenantId,
                Guid.Parse(loginResult.UserId),
                null,
                null,
                null),
            cancellationToken);

        return new LoginResponse(
            tokenResult.AccessToken,
            tokenResult.AccessTokenExpiresAtUtc,
            tokenResult.RefreshToken,
            tokenResult.RefreshTokenExpiresAtUtc);
    }
}
