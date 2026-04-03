using Knox.Application.Abstractions.Cqrs;
using Knox.Application.Abstractions.Security;

namespace Knox.Application.Features.Auth.Commands.RevokeRefreshToken;

public sealed record RevokeRefreshTokenCommand(
    Guid TenantId,
    string RefreshToken,
    string? RevokedBy = null) : ICommand<Unit>;

public sealed class RevokeRefreshTokenCommandHandler(
    IRefreshTokenService refreshTokenService,
    IAuditService auditService) 
    : ICommandHandler<RevokeRefreshTokenCommand, Unit>
{
    public async Task<Unit> HandleAsync(RevokeRefreshTokenCommand command, CancellationToken cancellationToken = default)
    {
        await refreshTokenService.RevokeAsync(
            new RevokeRefreshTokenRequest(
                command.RefreshToken,
                command.TenantId,
                command.RevokedBy,
                null),
            cancellationToken);

        await auditService.WriteSecurityEventAsync(
            new AuditSecurityEvent(
                EventType: "Auth.RefreshToken.Revoked",
                EntityName: "RefreshToken",
                EntityId: null,
                DetailsJson: null,
                TenantId: command.TenantId,
                UserId: null,
                CorrelationId: null,
                IpAddress: null,
                UserAgent: null),
            cancellationToken);

        return default;
    }
}
