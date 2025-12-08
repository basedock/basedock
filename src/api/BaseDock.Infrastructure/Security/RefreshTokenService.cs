namespace BaseDock.Infrastructure.Security;

using System.Security.Cryptography;
using System.Text.Json;
using BaseDock.Application.Abstractions.Security;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly RedisConnectionFactory _redisFactory;
    private readonly JwtSettings _settings;
    private const string RefreshTokenPrefix = "refresh:";
    private const string UserTokensPrefix = "user_tokens:";

    public RefreshTokenService(RedisConnectionFactory redisFactory, IOptions<JwtSettings> settings)
    {
        _redisFactory = redisFactory;
        _settings = settings.Value;
    }

    public async Task<string> GenerateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var token = GenerateSecureToken();
        var now = DateTime.UtcNow;
        var expiresAt = now.AddDays(_settings.RefreshTokenExpirationDays);

        var tokenData = new RefreshTokenData(userId, now, expiresAt);
        var db = _redisFactory.GetDatabase();

        var json = JsonSerializer.Serialize(tokenData);
        var expiry = TimeSpan.FromDays(_settings.RefreshTokenExpirationDays);

        await db.StringSetAsync(RefreshTokenPrefix + token, json, expiry);

        // Track token for user (for revoking all user tokens)
        await db.SetAddAsync(UserTokensPrefix + userId, token);
        await db.KeyExpireAsync(UserTokensPrefix + userId, expiry);

        return token;
    }

    public async Task<RefreshTokenData?> ValidateRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var db = _redisFactory.GetDatabase();
        var json = await db.StringGetAsync(RefreshTokenPrefix + token);

        if (json.IsNullOrEmpty)
        {
            return null;
        }

        var tokenData = JsonSerializer.Deserialize<RefreshTokenData>(json.ToString());

        if (tokenData is null || tokenData.ExpiresAt < DateTime.UtcNow)
        {
            await RevokeRefreshTokenAsync(token, cancellationToken);
            return null;
        }

        return tokenData;
    }

    public async Task<string> RotateRefreshTokenAsync(string oldToken, RefreshTokenData tokenData, CancellationToken cancellationToken = default)
    {
        // Token already validated by caller - no need to validate again
        // This prevents race conditions when multiple refresh calls happen simultaneously

        // Revoke old token
        await RevokeRefreshTokenAsync(oldToken, cancellationToken);

        // Generate new token for the same user
        return await GenerateRefreshTokenAsync(tokenData.UserId, cancellationToken);
    }

    public async Task RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var db = _redisFactory.GetDatabase();

        // Get user ID before deleting
        var json = await db.StringGetAsync(RefreshTokenPrefix + token);
        if (!json.IsNullOrEmpty)
        {
            var tokenData = JsonSerializer.Deserialize<RefreshTokenData>(json.ToString());
            if (tokenData is not null)
            {
                await db.SetRemoveAsync(UserTokensPrefix + tokenData.UserId, token);
            }
        }

        await db.KeyDeleteAsync(RefreshTokenPrefix + token);
    }

    public async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var db = _redisFactory.GetDatabase();
        var tokens = await db.SetMembersAsync(UserTokensPrefix + userId);

        foreach (var token in tokens)
        {
            await db.KeyDeleteAsync(RefreshTokenPrefix + token.ToString());
        }

        await db.KeyDeleteAsync(UserTokensPrefix + userId);
    }

    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
