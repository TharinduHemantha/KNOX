using System.Text.Json;
using SecureMultiTenant.Application.Abstractions.Cqrs;
using SecureMultiTenant.Application.Abstractions.Security;
using SecureMultiTenant.Application.Common.Exceptions;
using SecureMultiTenant.Application.Features.Auth.Commands.PasswordLogin;

namespace SecureMultiTenant.Application.Features.Auth.Commands.EntraExchange;

public sealed class EntraExchangeCommandHandler(
    IEntraTokenValidator entraTokenValidator,
    IIdentityService identityService,
    ITokenService tokenService,
    IRefreshTokenService refreshTokenService,
    IAuditService auditService,
    ITenantContext tenantContext)
    : ICommandHandler<EntraExchangeCommand, LoginResponse>
{
    public async Task<LoginResponse> HandleAsync(EntraExchangeCommand command, CancellationToken cancellationToken = default)
    {
        if (!tenantContext.IsResolved || tenantContext.TenantId is null || string.IsNullOrWhiteSpace(tenantContext.TenantSubdomain))
        {
            throw new TenantResolutionException("Tenant was not resolved.");
        }

        var tokenValidation = await entraTokenValidator.ValidateIdTokenAsync(command.IdToken, cancellationToken);
        if (!tokenValidation.Succeeded
            || string.IsNullOrWhiteSpace(tokenValidation.Subject)
            || string.IsNullOrWhiteSpace(tokenValidation.ObjectId)
            || string.IsNullOrWhiteSpace(tokenValidation.EntraTenantId)
            || string.IsNullOrWhiteSpace(tokenValidation.Email))
        {
            await auditService.WriteSecurityEventAsync(
                new AuditSecurityEvent(
                    "auth.entra.failed",
                    "ApplicationUser",
                    null,
                    JsonSerializer.Serialize(new { tokenValidation.ErrorCode, tokenValidation.ErrorDescription }),
                    tenantContext.TenantId,
                    null,
                    null,
                    null,
                    null),
                cancellationToken);

            throw new ForbiddenException("Invalid Entra token exchange request.");
        }

        var loginResult = await identityService.EntraLoginAsync(
            new EntraLoginRequest(
                tenantContext.TenantId.Value,
                tokenValidation.EntraTenantId,
                tokenValidation.ObjectId,
                tokenValidation.Subject,
                tokenValidation.Email,
                tokenValidation.DisplayName,
                tokenValidation.IdentityProvider,
                tokenValidation.PreferredUserName),
            cancellationToken);

        if (!loginResult.Succeeded || loginResult.UserId is null || loginResult.Email is null || loginResult.Roles is null || loginResult.Permissions is null)
        {
            await auditService.WriteSecurityEventAsync(
                new AuditSecurityEvent(
                    "auth.entra.provisioning_failed",
                    "ApplicationUser",
                    null,
                    JsonSerializer.Serialize(new { loginResult.ErrorCode, loginResult.ErrorDescription, tokenValidation.Email, tokenValidation.ObjectId }),
                    tenantContext.TenantId,
                    null,
                    null,
                    null,
                    null),
                cancellationToken);

            throw new ForbiddenException(loginResult.ErrorDescription ?? "Unable to exchange Entra identity.");
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
                "auth.entra.succeeded",
                "ApplicationUser",
                loginResult.UserId,
                JsonSerializer.Serialize(new
                {
                    tenantId = tenantContext.TenantId,
                    tenantSubdomain = tenantContext.TenantSubdomain,
                    tokenValidation.EntraTenantId,
                    tokenValidation.ObjectId,
                    tokenValidation.Email
                }),
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
