using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Knox.Application.Abstractions.Security;
using Knox.Infrastructure.Persistence;
using Knox.Infrastructure.Persistence.Entities;

namespace Knox.Infrastructure.Security.Identity;

public sealed class EntraInvitationService(AppDbContext dbContext) : IEntraInvitationService
{
    public async Task<CreateEntraInvitationResult> CreateInvitationAsync(CreateEntraInvitationRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var invitationCode = CreateInvitationCode();
        var invitation = new EntraInvitation
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            Email = request.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            EntraTenantId = string.IsNullOrWhiteSpace(request.EntraTenantId) ? null : request.EntraTenantId.Trim(),
            RoleName = string.IsNullOrWhiteSpace(request.RoleName) ? null : request.RoleName.Trim(),
            InvitationCodeHash = Hash(invitationCode),
            InvitedByUserId = request.InvitedByUserId,
            ExpiresAtUtc = request.ExpiresAtUtc,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        dbContext.EntraInvitations.Add(invitation);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateEntraInvitationResult(invitation.Id, invitation.Email, invitation.RoleName, invitation.ExpiresAtUtc, invitationCode);
    }

    public async Task<ConsumeEntraInvitationResult> ConsumeForJitProvisioningAsync(ConsumeEntraInvitationRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var now = DateTimeOffset.UtcNow;

        var invitation = await dbContext.EntraInvitations
            .Where(x => x.TenantId == request.TenantId
                        && x.NormalizedEmail == normalizedEmail
                        && !x.IsRevoked
                        && x.AcceptedAtUtc == null
                        && x.ExpiresAtUtc >= now)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (invitation is null)
        {
            return new ConsumeEntraInvitationResult(false, "invitation_required", "A valid invitation is required for first-time Entra onboarding.", null, null);
        }

        if (!string.IsNullOrWhiteSpace(invitation.EntraTenantId)
            && !string.Equals(invitation.EntraTenantId, request.EntraTenantId, StringComparison.OrdinalIgnoreCase))
        {
            return new ConsumeEntraInvitationResult(false, "entra_tenant_mismatch", "Invitation is not valid for this Entra tenant.", null, null);
        }

        invitation.AcceptedAtUtc = now;
        invitation.AcceptedEntraObjectId = request.EntraObjectId;
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ConsumeEntraInvitationResult(true, null, null, invitation.Id, invitation.RoleName);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();

    private static string CreateInvitationCode() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

    private static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
