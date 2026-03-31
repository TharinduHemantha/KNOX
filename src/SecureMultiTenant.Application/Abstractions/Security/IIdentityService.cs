namespace SecureMultiTenant.Application.Abstractions.Security;

public interface IIdentityService
{
    Task<LoginResult> PasswordLoginAsync(string userNameOrEmail, string password, Guid tenantId, CancellationToken cancellationToken = default);
    Task<LoginResult> EntraLoginAsync(EntraLoginRequest request, CancellationToken cancellationToken = default);
    Task<TokenRefreshIdentityResult> GetTokenRefreshIdentityAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
}

public sealed record EntraLoginRequest(
    Guid AppTenantId,
    string EntraTenantId,
    string EntraObjectId,
    string Subject,
    string Email,
    string? DisplayName,
    string? IdentityProvider,
    string? PreferredUserName);

public sealed record LoginResult(
    bool Succeeded,
    string? ErrorCode,
    string? ErrorDescription,
    string? UserId,
    string? Email,
    IReadOnlyCollection<string>? Roles,
    IReadOnlyCollection<string>? Permissions);

public sealed record TokenRefreshIdentityResult(
    bool Succeeded,
    string? ErrorCode,
    string? ErrorDescription,
    string? UserId,
    string? Email,
    Guid? TenantId,
    IReadOnlyCollection<string>? Roles,
    IReadOnlyCollection<string>? Permissions);
