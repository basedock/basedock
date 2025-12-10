namespace BaseDock.Api.Modules.Users;

using System.Security.Claims;
using BaseDock.Api.Extensions;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Users.Commands.CreateUser;
using BaseDock.Application.Features.Users.Commands.DeleteUser;
using BaseDock.Application.Features.Users.Commands.UpdateUser;
using BaseDock.Application.Features.Users.DTOs;
using BaseDock.Application.Features.Users.Queries.GetUserById;
using BaseDock.Application.Features.Users.Queries.GetUsers;
using Carter;

public class UserModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users");

        group.MapGet("/", GetUsers)
            .WithName("GetUsers")
            .WithSummary("Get all users (Admin only)")
            .Produces<IEnumerable<UserDto>>()
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/{id:guid}", GetUserById)
            .WithName("GetUserById")
            .WithSummary("Get user by ID (Admin only)")
            .Produces<UserDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPost("/", CreateUser)
            .WithName("CreateUser")
            .WithSummary("Create a new user (Admin only)")
            .Produces<UserDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPut("/{id:guid}", UpdateUser)
            .WithName("UpdateUser")
            .WithSummary("Update a user (Admin only)")
            .Produces<UserDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapDelete("/{id:guid}", DeleteUser)
            .WithName("DeleteUser")
            .WithSummary("Delete a user (Admin only)")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    private static bool IsAdmin(ClaimsPrincipal user)
    {
        var isAdminClaim = user.FindFirst("isAdmin")?.Value;
        return bool.TryParse(isAdminClaim, out var isAdmin) && isAdmin;
    }

    private static async Task<IResult> GetUsers(
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        if (!IsAdmin(user))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Error.Forbidden",
                detail: "Only administrators can view all users.");
        }

        var query = new GetUsersQuery(limit);
        var result = await dispatcher.QueryAsync(query, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetUserById(
        Guid id,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        if (!IsAdmin(user))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Error.Forbidden",
                detail: "Only administrators can view user details.");
        }

        var query = new GetUserByIdQuery(id);
        var result = await dispatcher.QueryAsync(query, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> CreateUser(
        CreateUserRequest request,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        if (!IsAdmin(user))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Error.Forbidden",
                detail: "Only administrators can create users.");
        }

        var command = new CreateUserCommand(request.Email, request.DisplayName, request.Password);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToCreatedResult($"/api/users/{result.Value?.Id}");
    }

    private static async Task<IResult> UpdateUser(
        Guid id,
        UpdateUserRequest request,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        if (!IsAdmin(user))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Error.Forbidden",
                detail: "Only administrators can update users.");
        }

        var command = new UpdateUserCommand(id, request.DisplayName, request.Email);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> DeleteUser(
        Guid id,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        if (!IsAdmin(user))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Error.Forbidden",
                detail: "Only administrators can delete users.");
        }

        // Prevent self-deletion
        var currentUserId = GetUserId(user);
        if (currentUserId == id)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Error.InvalidOperation",
                detail: "You cannot delete your own account.");
        }

        var command = new DeleteUserCommand(id);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToHttpResult();
    }
}
