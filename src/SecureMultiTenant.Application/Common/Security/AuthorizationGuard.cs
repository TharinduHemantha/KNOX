using SecureMultiTenant.Application.Abstractions.Security;
using SecureMultiTenant.Application.Common.Exceptions;

namespace SecureMultiTenant.Application.Common.Security;

public static class AuthorizationGuard
{
    public static void RequirePermission(ICurrentUserService currentUser, string permission)
    {
        if (!currentUser.Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase))
        {
            throw new ForbiddenException($"Missing permission: {permission}");
        }
    }
}
