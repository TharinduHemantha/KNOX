namespace Knox.Api.Options;

public sealed class TenantResolutionOptions
{
    public string RootHost { get; set; } = "api.com";
    public IReadOnlyCollection<string> ReservedSubdomains { get; set; } = ["www", "api", "admin", "localhost"];
}
