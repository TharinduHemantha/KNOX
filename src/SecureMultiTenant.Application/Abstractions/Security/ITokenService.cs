namespace SecureMultiTenant.Application.Abstractions.Security;

public interface ITokenService
{
    Task<TokenResult> CreateAccessAndRefreshTokensAsync(TokenRequest request, CancellationToken cancellationToken = default);
}

public sealed record TokenRequest(
    string UserId,
    string Email,
    Guid TenantId,
    string TenantSubdomain,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);

public sealed record TokenResult(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc,
    string RefreshTokenHash);
