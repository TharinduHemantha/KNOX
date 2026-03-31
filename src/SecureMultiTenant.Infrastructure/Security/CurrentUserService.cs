using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SecureMultiTenant.Application.Abstractions.Security;

namespace SecureMultiTenant.Infrastructure.Security;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public string? UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public string? Email => User?.FindFirstValue(ClaimTypes.Email);
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public bool IsInRole(string role) => User?.IsInRole(role) == true;

    public IReadOnlyCollection<string> Permissions =>
        User?.FindAll("permission").Select(x => x.Value).Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
        ?? Array.Empty<string>();
}
