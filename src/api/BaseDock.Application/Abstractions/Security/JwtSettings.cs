namespace BaseDock.Application.Abstractions.Security;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public required string Secret { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public int AccessTokenExpirationMinutes { get; init; } = 5;
    public int RefreshTokenExpirationDays { get; init; } = 7;
    public CookieSecuritySettings CookieSecurity { get; init; } = new();
}

public sealed class CookieSecuritySettings
{
    /// <summary>
    /// Whether to use Secure flag for cookies. Defaults to true (production-safe).
    /// Set to false in Development for HTTP support with Vite proxy.
    /// </summary>
    public bool UseSecure { get; init; } = true;

    /// <summary>
    /// SameSite cookie policy. Defaults to Strict for production security.
    /// Use Lax in Development for better compatibility with proxies.
    /// </summary>
    public string SameSite { get; init; } = "Strict";
}
