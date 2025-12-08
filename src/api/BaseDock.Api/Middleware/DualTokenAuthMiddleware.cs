namespace BaseDock.Api.Middleware;

using BaseDock.Application.Abstractions.Security;

public sealed class DualTokenAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DualTokenAuthMiddleware> _logger;

    private static readonly HashSet<string> PublicPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/auth/login",
        "/api/auth/refresh",
        "/api/auth/logout",
        "/openapi",
        "/scalar"
    };

    public DualTokenAuthMiddleware(RequestDelegate next, ILogger<DualTokenAuthMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IJwtService jwtService,
        IRefreshTokenService refreshTokenService,
        ITokenBlacklistService blacklistService)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Skip auth for public endpoints
        if (IsPublicPath(path))
        {
            await _next(context);
            return;
        }

        // Skip auth for non-API paths (static files, etc.)
        if (!path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Extract tokens
        var accessToken = ExtractBearerToken(context);
        var refreshToken = context.Request.Cookies["refresh_token"];

        // Validate access token
        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.LogWarning("Missing access token for path: {Path}", path);
            await WriteUnauthorizedResponse(context, "Missing access token");
            return;
        }

        var principal = jwtService.ValidateAccessToken(accessToken);
        if (principal is null)
        {
            _logger.LogWarning("Invalid access token for path: {Path}", path);
            await WriteUnauthorizedResponse(context, "Invalid access token");
            return;
        }

        // Check if token is blacklisted
        var jti = jwtService.GetTokenId(accessToken);
        if (!string.IsNullOrEmpty(jti) && await blacklistService.IsBlacklistedAsync(jti))
        {
            _logger.LogWarning("Blacklisted token used for path: {Path}", path);
            await WriteUnauthorizedResponse(context, "Token has been revoked");
            return;
        }

        // Validate refresh token (dual validation)
        if (string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogWarning("Missing refresh token for path: {Path}", path);
            await WriteUnauthorizedResponse(context, "Missing refresh token");
            return;
        }

        var refreshTokenData = await refreshTokenService.ValidateRefreshTokenAsync(refreshToken);
        if (refreshTokenData is null)
        {
            _logger.LogWarning("Invalid refresh token for path: {Path}", path);
            await WriteUnauthorizedResponse(context, "Invalid refresh token");
            return;
        }

        // Verify both tokens belong to the same user
        // Note: JwtSecurityTokenHandler maps "sub" to ClaimTypes.NameIdentifier
        var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId) || userId != refreshTokenData.UserId)
        {
            _logger.LogWarning("Token user mismatch for path: {Path}", path);
            await WriteUnauthorizedResponse(context, "Token mismatch");
            return;
        }

        // Set the authenticated user
        context.User = principal;

        await _next(context);
    }

    private static bool IsPublicPath(string path)
    {
        foreach (var publicPath in PublicPaths)
        {
            if (path.StartsWith(publicPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
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

    private static async Task WriteUnauthorizedResponse(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";

        var response = new
        {
            type = "https://tools.ietf.org/html/rfc7235#section-3.1",
            title = "Unauthorized",
            status = 401,
            detail = message
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}
