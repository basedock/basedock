namespace BaseDock.Application.Abstractions.Security;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public required string Secret { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public int AccessTokenExpirationMinutes { get; init; } = 5;
    public int RefreshTokenExpirationDays { get; init; } = 7;
}
