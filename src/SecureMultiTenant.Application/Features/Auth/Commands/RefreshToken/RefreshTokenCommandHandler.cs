using System.Text.Json;
using SecureMultiTenant.Application.Abstractions.Cqrs;
using SecureMultiTenant.Application.Abstractions.Security;
using SecureMultiTenant.Application.Common.Exceptions;

namespace SecureMultiTenant.Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    IRefreshTokenService refreshTokenService,
    IIdentityService identityService,
    ITokenService tokenService,
    IAuditService auditService,
    ITenantContext tenantContext)
    : ICommandHandler<RefreshTokenCommand, LoginResponse>
{
    public async Task<LoginResponse> HandleAsync(RefreshTokenCommand command, CancellationToken cancellationToken = default)
    {
        if (!tenantContext.IsResolved || tenantContext.TenantId is null || string.IsNullOrWhiteSpace(tenantContext.TenantSubdomain))
        {
            throw new TenantResolutionException("Tenant was not resolved.");
        }

        var tokenValidation = await refreshTokenService.ValidateAsync(
            command.RefreshToken,
            tenantContext.TenantId.Value,
            cancellationToken);

        if (!tokenValidation.Succeeded || tokenValidation.UserId is null)
        {
            await auditService.WriteSecurityEventAsync(
                new AuditSecurityEvent(
                    "auth.refresh.failed",
                    "RefreshToken",
                    null,
                    JsonSerializer.Serialize(new { tokenValidation.ErrorCode, tokenValidation.ErrorDescription }),
                    tenantContext.TenantId,
                    null,
                    null,
                    null,
                    null),
                cancellationToken);

            throw new ForbiddenException("Invalid refresh token request.");
        }

        var identityResult = await identityService.GetTokenRefreshIdentityAsync(
            tokenValidation.UserId.Value,
            tenantContext.TenantId.Value,
            cancellationToken);

        if (!identityResult.Succeeded || identityResult.UserId is null || identityResult.Email is null || identityResult.Roles is null || identityResult.Permissions is null)
        {
            throw new ForbiddenException("User is not eligible for token refresh.");
        }

        var tokenResult = await tokenService.CreateAccessAndRefreshTokensAsync(
            new TokenRequest(
                identityResult.UserId,
                identityResult.Email,
                tenantContext.TenantId.Value,
                tenantContext.TenantSubdomain,
                identityResult.Roles,
                identityResult.Permissions),
            cancellationToken);

        await refreshTokenService.RotateAsync(
            new RotateRefreshTokenRequest(
                command.RefreshToken,
                tokenResult.RefreshTokenHash,
                tokenResult.RefreshTokenExpiresAtUtc,
                identityResult.UserId,
                null),
            cancellationToken);

        await auditService.WriteSecurityEventAsync(
            new AuditSecurityEvent(
                "auth.refresh.succeeded",
                "RefreshToken",
                identityResult.UserId,
                JsonSerializer.Serialize(new { tenantId = tenantContext.TenantId, tenantSubdomain = tenantContext.TenantSubdomain }),
                tenantContext.TenantId,
                Guid.Parse(identityResult.UserId),
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
