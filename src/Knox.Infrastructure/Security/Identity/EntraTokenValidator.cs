using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Knox.Application.Abstractions.Security;

namespace Knox.Infrastructure.Security.Identity;

public sealed class EntraTokenValidator : IEntraTokenValidator
{
    private readonly EntraOptions _options;
    private readonly IConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public EntraTokenValidator(IOptions<EntraOptions> options)
    {
        _options = options.Value;
        var metadataAddress = string.IsNullOrWhiteSpace(_options.MetadataAddress)
            ? $"{_options.Instance.TrimEnd('/')}/{_options.TenantId}/v2.0/.well-known/openid-configuration"
            : _options.MetadataAddress;

        _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            metadataAddress,
            new OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever { RequireHttps = true });
    }

    public async Task<EntraTokenValidationResult> ValidateIdTokenAsync(string idToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            return EntraTokenValidationResult.Failure("invalid_id_token", "The Entra ID token is required.");
        }

        OpenIdConnectConfiguration configuration;
        try
        {
            configuration = await _configurationManager.GetConfigurationAsync(cancellationToken);
        }
        catch
        {
            return EntraTokenValidationResult.Failure("entra_metadata_unavailable", "Unable to load Entra metadata.");
        }

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = BuildValidIssuers(configuration),
            ValidateAudience = true,
            ValidAudience = _options.ClientId,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = configuration.SigningKeys,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2),
            RequireSignedTokens = true,
            RequireExpirationTime = true
        };

        try
        {
            var principal = _tokenHandler.ValidateToken(idToken, validationParameters, out _);
            var entraTenantId = principal.FindFirst("tid")?.Value;
            var objectId = principal.FindFirst("oid")?.Value;
            var subject = principal.FindFirst("sub")?.Value;
            var email = principal.FindFirst("preferred_username")?.Value
                ?? principal.FindFirst("email")?.Value
                ?? principal.FindFirst("upn")?.Value;
            var displayName = principal.FindFirst("name")?.Value;
            var identityProvider = principal.FindFirst("idp")?.Value;

            if (string.IsNullOrWhiteSpace(entraTenantId) || !string.Equals(entraTenantId, _options.TenantId, StringComparison.OrdinalIgnoreCase))
            {
                return EntraTokenValidationResult.Failure("entra_tenant_mismatch", "The token does not belong to the configured Entra tenant.");
            }

            if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(objectId) || string.IsNullOrWhiteSpace(email))
            {
                return EntraTokenValidationResult.Failure("entra_claims_incomplete", "The Entra token is missing required claims.");
            }

            return EntraTokenValidationResult.Success(
                subject,
                objectId,
                entraTenantId,
                email,
                displayName,
                principal.FindFirst("preferred_username")?.Value,
                identityProvider);
        }
        catch (SecurityTokenException ex)
        {
            return EntraTokenValidationResult.Failure("invalid_id_token", ex.Message);
        }
    }

    private IReadOnlyCollection<string> BuildValidIssuers(OpenIdConnectConfiguration configuration)
    {
        var issuers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(_options.ValidIssuer))
        {
            issuers.Add(_options.ValidIssuer);
        }

        if (!string.IsNullOrWhiteSpace(configuration.Issuer))
        {
            issuers.Add(configuration.Issuer);
        }

        issuers.Add($"{_options.Instance.TrimEnd('/')}/{_options.TenantId}/v2.0");
        return issuers.ToArray();
    }
}
