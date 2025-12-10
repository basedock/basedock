namespace BaseDock.Application.Features.Auth.Commands.Login;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Abstractions.Security;
using BaseDock.Application.Features.Users.Mappers;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class LoginCommandHandler(
    IApplicationDbContext db,
    IJwtService jwtService,
    IRefreshTokenService refreshTokenService,
    IPasswordHasher passwordHasher)
    : ICommandHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == command.Email.ToLowerInvariant(), cancellationToken);

        if (user is null)
        {
            return Result.Failure<LoginResponse>(
                Error.Unauthorized("Invalid email or password."));
        }

        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            return Result.Failure<LoginResponse>(
                Error.Unauthorized("Invalid email or password."));
        }

        var isValidPassword = passwordHasher.VerifyPassword(command.Password, user.PasswordHash);

        if (!isValidPassword)
        {
            return Result.Failure<LoginResponse>(
                Error.Unauthorized("Invalid email or password."));
        }

        // Generate JWT access token
        var accessToken = jwtService.GenerateAccessToken(user);
        var accessTokenExpiration = jwtService.GetTokenExpiration(accessToken);

        // Generate refresh token and store in Redis
        var refreshToken = await refreshTokenService.GenerateRefreshTokenAsync(user.Id, cancellationToken);

        var userDto = user.ToDto();

        return Result.Success(new LoginResponse(
            accessToken,
            accessTokenExpiration,
            refreshToken,
            userDto));
    }
}
