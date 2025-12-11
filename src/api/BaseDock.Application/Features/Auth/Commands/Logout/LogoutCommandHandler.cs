namespace BaseDock.Application.Features.Auth.Commands.Logout;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Abstractions.Security;
using BaseDock.Domain.Primitives;

public sealed class LogoutCommandHandler(
    IJwtService jwtService,
    IRefreshTokenService refreshTokenService,
    ITokenBlacklistService blacklistService,
    TimeProvider dateTime)
    : ICommandHandler<LogoutCommand, Result>
{
    private readonly TimeProvider _dateTime = dateTime;

    public async Task<Result> HandleAsync(
        LogoutCommand command,
        CancellationToken cancellationToken = default)
    {
        // Blacklist the access token
        if (!string.IsNullOrEmpty(command.AccessToken))
        {
            var jti = jwtService.GetTokenId(command.AccessToken);
            if (!string.IsNullOrEmpty(jti))
            {
                var expiration = jwtService.GetTokenExpiration(command.AccessToken);
                var remainingTime = expiration - _dateTime.GetUtcNow();

                if (remainingTime > TimeSpan.Zero)
                {
                    await blacklistService.BlacklistTokenAsync(jti, remainingTime, cancellationToken);
                }
            }
        }

        // Revoke the refresh token
        if (!string.IsNullOrEmpty(command.RefreshToken))
        {
            await refreshTokenService.RevokeRefreshTokenAsync(command.RefreshToken, cancellationToken);
        }

        return Result.Success();
    }
}
