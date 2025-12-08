namespace BaseDock.Application.Abstractions.Security;

public record RefreshTokenData(Guid UserId, DateTime CreatedAt, DateTime ExpiresAt);

public interface IRefreshTokenService
{
    Task<string> GenerateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<RefreshTokenData?> ValidateRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<string> RotateRefreshTokenAsync(string oldToken, RefreshTokenData tokenData, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
}
