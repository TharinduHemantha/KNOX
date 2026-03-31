using System.Text.Json;
using SecureMultiTenant.Application.Abstractions.Cqrs;
using SecureMultiTenant.Application.Abstractions.Security;
using SecureMultiTenant.Application.Common.Exceptions;

namespace SecureMultiTenant.Application.Features.Auth.Commands.RevokeRefreshToken;

public sealed class RevokeRefreshTokenCommandHandler(
    IRefreshTokenService refreshTokenService,
    IAuditService auditService,
    ICurrentUserService currentUserService,
    ITenantContext tenantContext)
    : ICommandHandler<RevokeRefreshTokenCommand, Unit>
{
    public async Task<Unit> HandleAsync(RevokeRefreshTokenCommand command, CancellationToken cancellationToken = default)
    {
        if (!tenantContext.IsResolved || tenantContext.TenantId is null)
        {
            throw new TenantResolutionException("Tenant was not resolved.");
        }

        await refreshTokenService.RevokeAsync(
            new RevokeRefreshTokenRequest(
                command.RefreshToken,
                tenantContext.TenantId.Value,
                currentUserService.UserId,
                null),
            cancellationToken);

        Guid? userId = Guid.TryParse(currentUserService.UserId, out var parsedUserId) ? parsedUserId : null;

        await auditService.WriteSecurityEventAsync(
            new AuditSecurityEvent(
                "auth.refresh.revoked",
                "RefreshToken",
                currentUserService.UserId,
                JsonSerializer.Serialize(new { tenantId = tenantContext.TenantId }),
                tenantContext.TenantId,
                userId,
                null,
                null,
                null),
            cancellationToken);

        return new Unit();
    }
}
