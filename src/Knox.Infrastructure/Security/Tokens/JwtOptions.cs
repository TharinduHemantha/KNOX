namespace Knox.Infrastructure.Security.Tokens;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "Knox.Api";
    public string Audience { get; set; } = "Knox.Client";
    public string SigningKey { get; set; } = "CHANGE_ME_TO_LONG_RANDOM_SECRET";
    public int AccessTokenMinutes { get; set; } = 30;
    public int RefreshTokenDays { get; set; } = 7;
}
