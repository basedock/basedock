namespace BaseDock.Api.Modules.Auth;

using BaseDock.Api.Extensions;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Auth.Commands.Login;
using Carter;

public class AuthModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Authenticate user and create session")
            .Produces<LoginResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        IDispatcher dispatcher,
        HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await dispatcher.SendAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToHttpResult();
        }

        var response = result.Value;

        httpContext.Response.Cookies.Append("session_id", response.SessionId.ToString(), new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = response.ExpiresAt
        });

        return Results.Ok(response);
    }
}
