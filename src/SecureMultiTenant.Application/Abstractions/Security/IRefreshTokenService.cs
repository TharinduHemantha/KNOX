namespace SecureMultiTenant.Application.Abstractions.Security;

public interface IRefreshTokenService
{
    Task StoreAsync(StoreRefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task<RefreshTokenValidationResult> ValidateAsync(string refreshToken, Guid tenantId, CancellationToken cancellationToken = default);
    Task RotateAsync(RotateRefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task RevokeAsync(RevokeRefreshTokenRequest request, CancellationToken cancellationToken = default);
}

public sealed record StoreRefreshTokenRequest(
    Guid UserId,
    Guid TenantId,
    string RefreshTokenHash,
    DateTimeOffset ExpiresAtUtc,
    string? CreatedByIp);

public sealed record RotateRefreshTokenRequest(
    string CurrentRefreshToken,
    string NewRefreshTokenHash,
    DateTimeOffset NewExpiresAtUtc,
    string? ReplacedBy,
    string? CreatedByIp);

public sealed record RevokeRefreshTokenRequest(
    string RefreshToken,
    Guid TenantId,
    string? RevokedBy,
    string? RevokedByIp);

public sealed record RefreshTokenValidationResult(
    bool Succeeded,
    string? ErrorCode,
    string? ErrorDescription,
    Guid? UserId,
    Guid? TenantId,
    DateTimeOffset? ExpiresAtUtc,
    bool IsRevoked)
{
    public static RefreshTokenValidationResult Invalid(string errorCode, string errorDescription)
        => new(false, errorCode, errorDescription, null, null, null, false);

    public static RefreshTokenValidationResult Valid(Guid userId, Guid tenantId, DateTimeOffset expiresAtUtc, bool isRevoked = false)
        => new(true, null, null, userId, tenantId, expiresAtUtc, isRevoked);
}
