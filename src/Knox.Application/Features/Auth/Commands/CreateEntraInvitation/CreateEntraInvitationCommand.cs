using Knox.Application.Abstractions.Cqrs;
using Knox.Application.Abstractions.Security;
using Knox.Application.Features.Auth.Common;

namespace Knox.Application.Features.Auth.Commands.CreateEntraInvitation;

public sealed record CreateEntraInvitationCommand(
    Guid TenantId,
    string Email,
    Guid InvitedByUserId,
    string? Role = null,
    string? EntraTenantId = null,
    int ExpirationDays = 7) : ICommand<CreateEntraInvitationResponse>;

public sealed class CreateEntraInvitationCommandHandler(IEntraInvitationService entraInvitationService) 
    : ICommandHandler<CreateEntraInvitationCommand, CreateEntraInvitationResponse>
{
    public async Task<CreateEntraInvitationResponse> HandleAsync(
        CreateEntraInvitationCommand command, 
        CancellationToken cancellationToken = default)
    {
        var result = await entraInvitationService.CreateInvitationAsync(
            new CreateEntraInvitationRequest(
                command.TenantId,
                command.Email,
                command.EntraTenantId,
                command.Role,
                command.InvitedByUserId,
                DateTimeOffset.UtcNow.AddDays(command.ExpirationDays)),
            cancellationToken);

        return new CreateEntraInvitationResponse(
            result.InvitationId, 
            result.InvitationCode, 
            result.ExpiresAtUtc);
    }
}
