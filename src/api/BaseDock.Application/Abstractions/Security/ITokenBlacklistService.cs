namespace BaseDock.Application.Abstractions.Security;

public interface ITokenBlacklistService
{
    Task BlacklistTokenAsync(string jti, TimeSpan expiry, CancellationToken cancellationToken = default);
    Task<bool> IsBlacklistedAsync(string jti, CancellationToken cancellationToken = default);
}
