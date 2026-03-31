using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Knox.Application.Abstractions.Security;

namespace Knox.Infrastructure.Security.Tokens;

public sealed class JwtTokenService(IOptions<JwtOptions> options) : ITokenService
{
    private readonly JwtOptions _options = options.Value;

    public Task<TokenResult> CreateAccessAndRefreshTokensAsync(TokenRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var accessExpires = now.AddMinutes(_options.AccessTokenMinutes);
        var refreshExpires = now.AddDays(_options.RefreshTokenDays);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, request.UserId),
            new(ClaimTypes.NameIdentifier, request.UserId),
            new(ClaimTypes.Email, request.Email),
            new("tenant_id", request.TenantId.ToString()),
            new("tenant_subdomain", request.TenantSubdomain)
        };

        claims.AddRange(request.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(request.Permissions.Select(permission => new Claim("permission", permission)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: accessExpires.UtcDateTime,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshTokenBytes = RandomNumberGenerator.GetBytes(64);
        var refreshToken = Convert.ToBase64String(refreshTokenBytes);
        var refreshTokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));

        return Task.FromResult(new TokenResult(accessToken, accessExpires, refreshToken, refreshExpires, refreshTokenHash));
    }
}
