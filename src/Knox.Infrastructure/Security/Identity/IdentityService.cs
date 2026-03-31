using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Knox.Application.Abstractions.Security;
using Knox.Infrastructure.Identity;

namespace Knox.Infrastructure.Security.Identity;

public sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    SignInManager<ApplicationUser> signInManager,
    IOptions<EntraOptions> entraOptions,
    IEntraInvitationService entraInvitationService)
    : IIdentityService
{
    private readonly EntraOptions _entraOptions = entraOptions.Value;

    public async Task<LoginResult> PasswordLoginAsync(string userNameOrEmail, string password, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByNameAsync(userNameOrEmail)
                   ?? await userManager.FindByEmailAsync(userNameOrEmail);

        if (user is null || user.TenantId != tenantId || !user.IsActive || user.IsSoftDeleted)
        {
            return Invalid();
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            return Invalid();
        }

        user.LastLoginAtUtc = DateTimeOffset.UtcNow;
        await userManager.UpdateAsync(user);

        return await BuildLoginResultAsync(user);
    }

    public async Task<LoginResult> EntraLoginAsync(EntraLoginRequest request, CancellationToken cancellationToken = default)
    {
        if (!_entraOptions.EnableJitProvisioning)
        {
            return Invalid("jit_disabled", "Entra JIT provisioning is disabled.");
        }

        if (_entraOptions.RequireEmailClaim && string.IsNullOrWhiteSpace(request.Email))
        {
            return Invalid("email_required", "A routable email claim is required for Entra sign-in.");
        }

        var user = await userManager.Users
            .FirstOrDefaultAsync(
                x => x.TenantId == request.AppTenantId
                     && x.EntraObjectId == request.EntraObjectId
                     && !x.IsSoftDeleted,
                cancellationToken);

        if (user is null)
        {
            user = await userManager.Users
                .FirstOrDefaultAsync(
                    x => x.TenantId == request.AppTenantId
                         && x.NormalizedEmail == userManager.NormalizeEmail(request.Email)
                         && !x.IsSoftDeleted,
                    cancellationToken);

            if (user is not null && !string.Equals(user.AuthenticationSource, "Entra", StringComparison.OrdinalIgnoreCase))
            {
                return Invalid("email_conflict", "A local account already exists with the same email address.");
            }
        }

        string? invitationRoleName = null;

        if (user is null && _entraOptions.RequireInvitationForJitProvisioning)
        {
            var invitationResult = await entraInvitationService.ConsumeForJitProvisioningAsync(
                new ConsumeEntraInvitationRequest(
                    request.AppTenantId,
                    request.Email,
                    request.EntraTenantId,
                    request.EntraObjectId),
                cancellationToken);

            if (!invitationResult.Succeeded)
            {
                return Invalid(invitationResult.ErrorCode, invitationResult.ErrorDescription);
            }

            invitationRoleName = invitationResult.RoleName;
        }

        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                TenantId = request.AppTenantId,
                UserName = BuildUserName(request),
                Email = request.Email,
                EmailConfirmed = true,
                DisplayName = request.DisplayName,
                AuthenticationSource = "Entra",
                EntraObjectId = request.EntraObjectId,
                EntraTenantId = request.EntraTenantId,
                ExternalSubject = request.Subject,
                IsActive = true,
                LockoutEnabled = true,
                LastLoginAtUtc = DateTimeOffset.UtcNow
            };

            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return Invalid("jit_create_failed", string.Join("; ", createResult.Errors.Select(x => x.Description)));
            }

            var defaultRoleName = !string.IsNullOrWhiteSpace(invitationRoleName) ? invitationRoleName : _entraOptions.DefaultJitRole;
            var role = await ResolveRoleAsync(defaultRoleName, request.AppTenantId, cancellationToken);
            if (role is not null)
            {
                var roleAssignResult = await userManager.AddToRoleAsync(user, role.Name!);
                if (!roleAssignResult.Succeeded)
                {
                    return Invalid("jit_role_assignment_failed", string.Join("; ", roleAssignResult.Errors.Select(x => x.Description)));
                }
            }
        }
        else
        {
            if (!user.IsActive)
            {
                return Invalid("user_inactive", "The user account is inactive.");
            }

            user.DisplayName = request.DisplayName ?? user.DisplayName;
            user.Email = request.Email;
            user.NormalizedEmail = userManager.NormalizeEmail(request.Email);
            user.AuthenticationSource = "Entra";
            user.EntraObjectId = request.EntraObjectId;
            user.EntraTenantId = request.EntraTenantId;
            user.ExternalSubject = request.Subject;
            user.LastLoginAtUtc = DateTimeOffset.UtcNow;

            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                user.UserName = BuildUserName(request);
                user.NormalizedUserName = userManager.NormalizeName(user.UserName);
            }

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return Invalid("jit_update_failed", string.Join("; ", updateResult.Errors.Select(x => x.Description)));
            }
        }

        return await BuildLoginResultAsync(user);
    }

    public async Task<TokenRefreshIdentityResult> GetTokenRefreshIdentityAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null || user.TenantId != tenantId || !user.IsActive || user.IsSoftDeleted)
        {
            return InvalidForRefresh();
        }

        var roles = await userManager.GetRolesAsync(user);
        var claims = await userManager.GetClaimsAsync(user);
        var permissions = claims.Where(x => x.Type == "permission").Select(x => x.Value).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        return new TokenRefreshIdentityResult(true, null, null, user.Id.ToString(), user.Email, tenantId, roles.ToArray(), permissions);
    }

    private async Task<LoginResult> BuildLoginResultAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var roleClaims = new List<Claim>();

        foreach (var roleName in roles)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is not null)
            {
                var claims = await roleManager.GetClaimsAsync(role);
                roleClaims.AddRange(claims);
            }
        }

        var userClaims = await userManager.GetClaimsAsync(user);
        var permissions = userClaims
            .Concat(roleClaims)
            .Where(x => x.Type == "permission")
            .Select(x => x.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new LoginResult(true, null, null, user.Id.ToString(), user.Email, roles.ToArray(), permissions);
    }

    private async Task<ApplicationRole?> ResolveRoleAsync(string roleName, Guid tenantId, CancellationToken cancellationToken)
    {
        return await roleManager.Roles
            .FirstOrDefaultAsync(
                x => x.Name == roleName && (x.TenantId == null || x.TenantId == tenantId),
                cancellationToken);
    }

    private static string BuildUserName(EntraLoginRequest request)
    {
        var seed = !string.IsNullOrWhiteSpace(request.PreferredUserName)
            ? request.PreferredUserName
            : request.Email;

        var safeSeed = seed.Replace("@", ".").Replace(" ", string.Empty, StringComparison.Ordinal).ToLowerInvariant();
        return $"entra.{safeSeed}.{request.EntraObjectId[..Math.Min(8, request.EntraObjectId.Length)]}";
    }

    private static LoginResult Invalid(string? code = "invalid_credentials", string? description = "Invalid login request.")
        => new(false, code, description, null, null, null, null);

    private static TokenRefreshIdentityResult InvalidForRefresh(string? code = "invalid_refresh_request", string? description = "Refresh token is not valid for this user.")
        => new(false, code, description, null, null, null, null, null);
}
