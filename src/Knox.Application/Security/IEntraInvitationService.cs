namespace Knox.Application.Abstractions.Security;

public interface IEntraInvitationService
{
    Task<CreateEntraInvitationResult> CreateInvitationAsync(CreateEntraInvitationRequest request, CancellationToken cancellationToken = default);
    Task<ConsumeEntraInvitationResult> ConsumeForJitProvisioningAsync(ConsumeEntraInvitationRequest request, CancellationToken cancellationToken = default);
}

public sealed record CreateEntraInvitationRequest(
    Guid TenantId,
    string Email,
    string? EntraTenantId,
    string? RoleName,
    Guid InvitedByUserId,
    DateTimeOffset ExpiresAtUtc);

public sealed record CreateEntraInvitationResult(
    Guid InvitationId,
    string Email,
    string? RoleName,
    DateTimeOffset ExpiresAtUtc,
    string InvitationCode);

public sealed record ConsumeEntraInvitationRequest(
    Guid TenantId,
    string Email,
    string EntraTenantId,
    string? EntraObjectId);

public sealed record ConsumeEntraInvitationResult(
    bool Succeeded,
    string? ErrorCode,
    string? ErrorDescription,
    Guid? InvitationId,
    string? RoleName);
