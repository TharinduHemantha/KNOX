using Knox.Application.Abstractions.Cqrs;
using Knox.Application.Abstractions.Security;
using Knox.Application.Features.Auth.Common;

namespace Knox.Application.Features.Auth.Commands.PasswordLogin;

public sealed record PasswordLoginCommand(
    Guid TenantId,
    string TenantSubdomain,
    string Email,
    string Password) : ICommand<LoginResponse>;

public sealed class PasswordLoginCommandHandler(
    IIdentityService identityService,
    ITokenService tokenService,
    IRefreshTokenService refreshTokenService) 
    : ICommandHandler<PasswordLoginCommand, LoginResponse>
{
    public async Task<LoginResponse> HandleAsync(PasswordLoginCommand command, CancellationToken cancellationToken = default)
    {
        var loginResult = await identityService.PasswordLoginAsync(
            command.Email, 
            command.Password, 
            command.TenantId, 
            cancellationToken);

        if (!loginResult.Succeeded || loginResult.UserId is null)
        {
            throw new UnauthorizedAccessException(loginResult.ErrorDescription ?? "Invalid credentials.");
        }

        var tokenResult = await tokenService.CreateAccessAndRefreshTokensAsync(
            new TokenRequest(
                loginResult.UserId,
                loginResult.Email!,
                command.TenantId,
                command.TenantSubdomain,
                loginResult.Roles ?? [],
                loginResult.Permissions ?? []),
            cancellationToken);

        await refreshTokenService.StoreAsync(
            new StoreRefreshTokenRequest(
                Guid.Parse(loginResult.UserId),
                command.TenantId,
                tokenResult.RefreshTokenHash,
                tokenResult.RefreshTokenExpiresAtUtc,
                null),
            cancellationToken);

        return new LoginResponse(
            tokenResult.AccessToken, 
            tokenResult.RefreshToken, 
            tokenResult.AccessTokenExpiresAtUtc);
    }
}
