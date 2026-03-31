using Knox.Application.Abstractions.Cqrs;
using Knox.Application.Abstractions.Security;
using Knox.Application.Features.Auth.Common;

namespace Knox.Application.Features.Auth.Commands.EntraExchange;

public sealed record EntraExchangeCommand(
    Guid TenantId,
    string TenantSubdomain,
    string EntraIdToken) : ICommand<LoginResponse>;

public sealed class EntraExchangeCommandHandler(
    IEntraTokenValidator entraTokenValidator,
    IIdentityService identityService,
    ITokenService tokenService,
    IRefreshTokenService refreshTokenService) 
    : ICommandHandler<EntraExchangeCommand, LoginResponse>
{
    public async Task<LoginResponse> HandleAsync(EntraExchangeCommand command, CancellationToken cancellationToken = default)
    {
        var validationResult = await entraTokenValidator.ValidateIdTokenAsync(command.EntraIdToken, cancellationToken);

        if (!validationResult.Succeeded || validationResult.Email is null)
        {
            throw new UnauthorizedAccessException(validationResult.ErrorDescription ?? "Invalid Entra ID token.");
        }

        var loginResult = await identityService.EntraLoginAsync(
            new EntraLoginRequest(
                command.TenantId,
                validationResult.EntraTenantId!,
                validationResult.ObjectId!,
                validationResult.Subject!,
                validationResult.Email,
                validationResult.DisplayName,
                validationResult.IdentityProvider,
                validationResult.PreferredUserName),
            cancellationToken);

        if (!loginResult.Succeeded || loginResult.UserId is null)
        {
            throw new UnauthorizedAccessException(loginResult.ErrorDescription ?? "Entra login failed.");
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
