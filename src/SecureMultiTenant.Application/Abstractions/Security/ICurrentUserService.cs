namespace SecureMultiTenant.Application.Abstractions.Security;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
    IReadOnlyCollection<string> Permissions { get; }
}
