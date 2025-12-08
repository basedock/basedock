namespace BaseDock.Application.Features.Auth.Commands.Refresh;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Abstractions.Security;
using BaseDock.Application.Features.Users.Mappers;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class RefreshCommandHandler(
    IApplicationDbContext db,
    IJwtService jwtService,
    IRefreshTokenService refreshTokenService)
    : ICommandHandler<RefreshCommand, Result<RefreshResponse>>
{
    public async Task<Result<RefreshResponse>> HandleAsync(
        RefreshCommand command,
        CancellationToken cancellationToken = default)
    {
        // Validate the refresh token
        var tokenData = await refreshTokenService.ValidateRefreshTokenAsync(
            command.RefreshToken, cancellationToken);

        if (tokenData is null)
        {
            return Result.Failure<RefreshResponse>(
                Error.Unauthorized("Invalid or expired refresh token."));
        }

        // Get the user
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == tokenData.UserId, cancellationToken);

        if (user is null)
        {
            // User was deleted, revoke the token
            await refreshTokenService.RevokeRefreshTokenAsync(command.RefreshToken, cancellationToken);
            return Result.Failure<RefreshResponse>(
                Error.Unauthorized("User not found."));
        }

        // Rotate the refresh token (generate new, delete old)
        // Pass pre-validated tokenData to avoid race conditions with double validation
        var newRefreshToken = await refreshTokenService.RotateRefreshTokenAsync(
            command.RefreshToken, tokenData, cancellationToken);

        // Generate new access token
        var accessToken = jwtService.GenerateAccessToken(user);
        var accessTokenExpiration = jwtService.GetTokenExpiration(accessToken);

        var userDto = user.ToDto();

        return Result.Success(new RefreshResponse(
            accessToken,
            accessTokenExpiration,
            newRefreshToken,
            userDto));
    }
}
