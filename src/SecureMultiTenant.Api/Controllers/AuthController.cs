using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureMultiTenant.Application.Abstractions.Cqrs;
using SecureMultiTenant.Application.Features.Auth.Commands.CreateEntraInvitation;
using SecureMultiTenant.Application.Features.Auth.Commands.EntraExchange;
using SecureMultiTenant.Application.Features.Auth.Commands.PasswordLogin;
using SecureMultiTenant.Application.Features.Auth.Commands.RefreshToken;
using SecureMultiTenant.Application.Features.Auth.Commands.RevokeRefreshToken;

namespace SecureMultiTenant.Api.Controllers;

[ApiVersion("1.0")]
public sealed class AuthController(ICommandDispatcher commandDispatcher) : BaseApiController
{
    [HttpPost("local/login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> LocalLogin([FromBody] PasswordLoginCommand request, CancellationToken cancellationToken)
    {
        var response = await commandDispatcher.DispatchAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var response = await commandDispatcher.DispatchAsync(request, cancellationToken);
        return Ok(response);
    }

    [Authorize]
    [HttpPost("refresh/revoke")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RevokeRefreshToken([FromBody] RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        await commandDispatcher.DispatchAsync(request, cancellationToken);
        return NoContent();
    }


    [Authorize]
    [HttpPost("entra/invitations")]
    [ProducesResponseType(typeof(CreateEntraInvitationResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateEntraInvitation([FromBody] CreateEntraInvitationCommand request, CancellationToken cancellationToken)
    {
        var response = await commandDispatcher.DispatchAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("entra/exchange")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> EntraExchange([FromBody] EntraExchangeCommand request, CancellationToken cancellationToken)
    {
        var response = await commandDispatcher.DispatchAsync(request, cancellationToken);
        return Ok(response);
    }
}
