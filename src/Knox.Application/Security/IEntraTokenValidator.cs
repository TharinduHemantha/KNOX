namespace Knox.Application.Abstractions.Security;

public interface IEntraTokenValidator
{
    Task<EntraTokenValidationResult> ValidateIdTokenAsync(string idToken, CancellationToken cancellationToken = default);
}

public sealed record EntraTokenValidationResult(
    bool Succeeded,
    string? ErrorCode,
    string? ErrorDescription,
    string? Subject,
    string? ObjectId,
    string? EntraTenantId,
    string? Email,
    string? DisplayName,
    string? PreferredUserName,
    string? IdentityProvider)
{
    public static EntraTokenValidationResult Failure(string code, string description)
        => new(false, code, description, null, null, null, null, null, null, null);

    public static EntraTokenValidationResult Success(
        string subject,
        string objectId,
        string entraTenantId,
        string email,
        string? displayName,
        string? preferredUserName,
        string? identityProvider)
        => new(true, null, null, subject, objectId, entraTenantId, email, displayName, preferredUserName, identityProvider);
}
