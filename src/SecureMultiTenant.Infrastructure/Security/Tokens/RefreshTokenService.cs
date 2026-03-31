using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SecureMultiTenant.Application.Abstractions.Security;
using SecureMultiTenant.Infrastructure.Persistence;
using SecureMultiTenant.Infrastructure.Persistence.Entities;

namespace SecureMultiTenant.Infrastructure.Security.Tokens;

public sealed class RefreshTokenService(AppDbContext dbContext) : IRefreshTokenService
{
    public async Task StoreAsync(StoreRefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            TenantId = request.TenantId,
            TokenHash = request.RefreshTokenHash,
            ExpiresAtUtc = request.ExpiresAtUtc,
            CreatedByIp = request.CreatedByIp,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<RefreshTokenValidationResult> ValidateAsync(string refreshToken, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tokenHash = Hash(refreshToken);

        var entity = await dbContext.RefreshTokens
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.TokenHash == tokenHash && x.TenantId == tenantId, cancellationToken);

        if (entity is null)
        {
            return RefreshTokenValidationResult.Invalid("refresh_token_not_found", "Refresh token was not found.");
        }

        if (entity.RevokedAtUtc.HasValue)
        {
            return RefreshTokenValidationResult.Invalid("refresh_token_revoked", "Refresh token has already been revoked.");
        }

        if (entity.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            return RefreshTokenValidationResult.Invalid("refresh_token_expired", "Refresh token has expired.");
        }

        return RefreshTokenValidationResult.Valid(entity.UserId, entity.TenantId, entity.ExpiresAtUtc);
    }

    public async Task RotateAsync(RotateRefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var currentTokenHash = Hash(request.CurrentRefreshToken);

        var current = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(x => x.TokenHash == currentTokenHash, cancellationToken)
            ?? throw new InvalidOperationException("Refresh token could not be found for rotation.");

        if (current.RevokedAtUtc.HasValue)
        {
            throw new InvalidOperationException("Refresh token has already been revoked.");
        }

        current.RevokedAtUtc = DateTimeOffset.UtcNow;
        current.RevokedBy = request.ReplacedBy;
        current.ReplacedByTokenHash = request.NewRefreshTokenHash;

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = current.UserId,
            TenantId = current.TenantId,
            TokenHash = request.NewRefreshTokenHash,
            ExpiresAtUtc = request.NewExpiresAtUtc,
            CreatedByIp = request.CreatedByIp,
            CreatedAtUtc = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAsync(RevokeRefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var tokenHash = Hash(request.RefreshToken);

        var current = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(x => x.TokenHash == tokenHash && x.TenantId == request.TenantId, cancellationToken)
            ?? throw new InvalidOperationException("Refresh token could not be found for revocation.");

        if (!current.RevokedAtUtc.HasValue)
        {
            current.RevokedAtUtc = DateTimeOffset.UtcNow;
            current.RevokedBy = request.RevokedBy;
            current.CreatedByIp = request.RevokedByIp ?? current.CreatedByIp;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static string Hash(string refreshToken)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
}
