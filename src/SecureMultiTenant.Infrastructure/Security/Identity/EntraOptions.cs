namespace SecureMultiTenant.Infrastructure.Security.Identity;

public sealed class EntraOptions
{
    public const string SectionName = "Authentication:Entra";
    public string TenantId { get; set; } = default!;
    public string ClientId { get; set; } = default!;
    public string Instance { get; set; } = "https://login.microsoftonline.com";
    public string? ValidIssuer { get; set; }
    public string? MetadataAddress { get; set; }
    public bool EnableJitProvisioning { get; set; } = true;
    public string DefaultJitRole { get; set; } = "ApplicationUser";
    public bool RequireEmailClaim { get; set; } = true;
    public bool RequireInvitationForJitProvisioning { get; set; } = true;
}
