using SecureMultiTenant.Application.Abstractions.Cqrs;
using SecureMultiTenant.Application.Abstractions.Security;
using SecureMultiTenant.Application.Common.Exceptions;
using SecureMultiTenant.Application.Common.Security;
using SecureMultiTenant.Domain.Security;

namespace SecureMultiTenant.Application.Features.Auth.Commands.CreateEntraInvitation;

public sealed class CreateEntraInvitationCommandHandler(
    ICurrentUserService currentUserService,
    ITenantContext tenantContext,
    IEntraInvitationService entraInvitationService)
    : ICommandHandler<CreateEntraInvitationCommand, CreateEntraInvitationResponse>
{
    public async Task<CreateEntraInvitationResponse> HandleAsync(CreateEntraInvitationCommand command, CancellationToken cancellationToken = default)
    {
        if (!tenantContext.IsResolved || tenantContext.TenantId is null)
        {
            throw new TenantResolutionException("Tenant was not resolved.");
        }

        if (!currentUserService.IsAuthenticated)
        {
            throw new ForbiddenException("Authenticated user is required.");
        }

        if (!currentUserService.IsInRole("SuperAdmin") && !currentUserService.IsInRole("TenantAdmin"))
        {
            AuthorizationGuard.RequirePermission(currentUserService, PermissionNames.UsersManage);
        }

        if (!Guid.TryParse(currentUserService.UserId, out var invitedByUserId))
        {
            throw new ForbiddenException("Authenticated user id is invalid.");
        }

        var result = await entraInvitationService.CreateInvitationAsync(
            new CreateEntraInvitationRequest(
                tenantContext.TenantId.Value,
                command.Email,
                command.EntraTenantId,
                command.RoleName,
                invitedByUserId,
                DateTimeOffset.UtcNow.AddDays(command.ExpiryDays)),
            cancellationToken);

        return new CreateEntraInvitationResponse(
            result.InvitationId,
            result.Email,
            result.RoleName,
            result.ExpiresAtUtc,
            result.InvitationCode);
    }
}
