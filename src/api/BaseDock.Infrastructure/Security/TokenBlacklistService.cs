namespace BaseDock.Infrastructure.Security;

using BaseDock.Application.Abstractions.Security;

public sealed class TokenBlacklistService : ITokenBlacklistService
{
    private readonly RedisConnectionFactory _redisFactory;
    private const string BlacklistPrefix = "blacklist:";

    public TokenBlacklistService(RedisConnectionFactory redisFactory)
    {
        _redisFactory = redisFactory;
    }

    public async Task BlacklistTokenAsync(string jti, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(jti))
        {
            return;
        }

        var db = _redisFactory.GetDatabase();
        await db.StringSetAsync(BlacklistPrefix + jti, "1", expiry);
    }

    public async Task<bool> IsBlacklistedAsync(string jti, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(jti))
        {
            return false;
        }

        var db = _redisFactory.GetDatabase();
        return await db.KeyExistsAsync(BlacklistPrefix + jti);
    }
}
