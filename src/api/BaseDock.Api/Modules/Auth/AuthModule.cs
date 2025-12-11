namespace BaseDock.Api.Modules.Auth;

using BaseDock.Api.Extensions;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Abstractions.Security;
using BaseDock.Application.Features.Auth.Commands.Login;
using BaseDock.Application.Features.Auth.Commands.Logout;
using BaseDock.Application.Features.Auth.Commands.Refresh;
using BaseDock.Application.Features.Users.DTOs;
using Carter;
using Microsoft.Extensions.Options;

public class AuthModule : ICarterModule
{
    private const string RefreshTokenCookieName = "refresh_token";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Authenticate user and return access token")
            .Produces<LoginApiResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/refresh", Refresh)
            .WithName("RefreshToken")
            .WithSummary("Refresh access token using refresh token cookie")
            .Produces<RefreshApiResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/logout", Logout)
            .WithName("Logout")
            .WithSummary("Logout and invalidate tokens")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        IDispatcher dispatcher,
        HttpContext httpContext,
        IOptions<JwtSettings> jwtSettings,
        TimeProvider dateTime,
        CancellationToken cancellationToken = default)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await dispatcher.SendAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToHttpResult();
        }

        var response = result.Value;

        // Set refresh token in HTTP-only cookie
        SetRefreshTokenCookie(httpContext, response.RefreshToken, jwtSettings.Value, dateTime);

        // Return only access token and user info (not the refresh token)
        return Results.Ok(new LoginApiResponse(
            response.AccessToken,
            response.AccessTokenExpiresAt,
            response.User));
    }

    private static async Task<IResult> Refresh(
        IDispatcher dispatcher,
        HttpContext httpContext,
        IOptions<JwtSettings> jwtSettings,
        TimeProvider dateTime,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = httpContext.Request.Cookies[RefreshTokenCookieName];

        if (string.IsNullOrEmpty(refreshToken))
        {
            return Results.Problem(
                detail: "Missing refresh token",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var command = new RefreshCommand(refreshToken);
        var result = await dispatcher.SendAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            // Clear invalid cookie
            ClearRefreshTokenCookie(httpContext);
            return result.ToHttpResult();
        }

        var response = result.Value;

        // Set new refresh token in cookie (rotation)
        SetRefreshTokenCookie(httpContext, response.NewRefreshToken, jwtSettings.Value, dateTime);

        return Results.Ok(new RefreshApiResponse(
            response.AccessToken,
            response.AccessTokenExpiresAt,
            response.User));
    }

    private static async Task<IResult> Logout(
        IDispatcher dispatcher,
        HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = httpContext.Request.Cookies[RefreshTokenCookieName];
        var accessToken = ExtractBearerToken(httpContext);

        var command = new LogoutCommand(accessToken ?? string.Empty, refreshToken ?? string.Empty);
        await dispatcher.SendAsync(command, cancellationToken);

        // Clear refresh token cookie
        ClearRefreshTokenCookie(httpContext);

        return Results.NoContent();
    }

    private static void SetRefreshTokenCookie(HttpContext httpContext, string token, JwtSettings settings, TimeProvider dateTime)
    {
        var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        httpContext.Response.Cookies.Append(RefreshTokenCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = isDevelopment ? SameSiteMode.None : SameSiteMode.Strict,
            Path = "/api",
            Expires = dateTime.GetUtcNow().AddDays(settings.RefreshTokenExpirationDays)
        });
    }

    private static void ClearRefreshTokenCookie(HttpContext httpContext)
    {
        var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        httpContext.Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = isDevelopment ? SameSiteMode.None : SameSiteMode.Strict,
            Path = "/api"
        });
    }

    private static string? ExtractBearerToken(HttpContext context)
    {
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return authHeader["Bearer ".Length..].Trim();
    }
}

// Response types for the API
public sealed record LoginApiResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    UserDto User);

public sealed record RefreshApiResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    UserDto User);
