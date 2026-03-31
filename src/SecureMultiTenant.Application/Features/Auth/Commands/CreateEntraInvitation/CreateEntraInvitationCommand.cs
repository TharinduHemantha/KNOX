using SecureMultiTenant.Application.Abstractions.Cqrs;

namespace SecureMultiTenant.Application.Features.Auth.Commands.CreateEntraInvitation;

public sealed record CreateEntraInvitationCommand(
    string Email,
    string? EntraTenantId,
    string? RoleName,
    int ExpiryDays = 7) : ICommand<CreateEntraInvitationResponse>;

public sealed record CreateEntraInvitationResponse(
    Guid InvitationId,
    string Email,
    string? RoleName,
    DateTimeOffset ExpiresAtUtc,
    string InvitationCode);
