using Knox.Application.Abstractions.Cqrs;
using Knox.Application.Abstractions.Security;
using Knox.Application.Features.Auth.Common;

namespace Knox.Application.Features.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand(
    Guid TenantId,
    string TenantSubdomain,
    string RefreshToken) : ICommand<LoginResponse>;

public sealed class RefreshTokenCommandHandler(
    IRefreshTokenService refreshTokenService,
    IIdentityService identityService,
    ITokenService tokenService) 
    : ICommandHandler<RefreshTokenCommand, LoginResponse>
{
    public async Task<LoginResponse> HandleAsync(RefreshTokenCommand command, CancellationToken cancellationToken = default)
    {
        var validationResult = await refreshTokenService.ValidateAsync(
            command.RefreshToken, 
            command.TenantId, 
            cancellationToken);

        if (!validationResult.Succeeded || validationResult.UserId is null)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        var identityResult = await identityService.GetTokenRefreshIdentityAsync(
            validationResult.UserId.Value, 
            command.TenantId, 
            cancellationToken);

        if (!identityResult.Succeeded)
        {
            throw new UnauthorizedAccessException("User not found or inactive.");
        }

        var tokenResult = await tokenService.CreateAccessAndRefreshTokensAsync(
            new TokenRequest(
                validationResult.UserId.Value.ToString(),
                identityResult.Email!,
                command.TenantId,
                command.TenantSubdomain,
                identityResult.Roles ?? [],
                identityResult.Permissions ?? []),
            cancellationToken);

        await refreshTokenService.RotateAsync(
            new RotateRefreshTokenRequest(
                command.RefreshToken,
                tokenResult.RefreshTokenHash,
                tokenResult.RefreshTokenExpiresAtUtc,
                tokenResult.RefreshToken,
                null),
            cancellationToken);

        return new LoginResponse(
            tokenResult.AccessToken, 
            tokenResult.RefreshToken, 
            tokenResult.AccessTokenExpiresAtUtc);
    }
}
