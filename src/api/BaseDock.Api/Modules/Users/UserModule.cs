namespace BaseDock.Api.Modules.Users;

using BaseDock.Api.Extensions;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Users.Commands.CreateUser;
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
            .WithSummary("Get all users")
            .Produces<IEnumerable<UserDto>>();

        group.MapGet("/{id:guid}", GetUserById)
            .WithName("GetUserById")
            .WithSummary("Get user by ID")
            .Produces<UserDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateUser)
            .WithName("CreateUser")
            .WithSummary("Create a new user")
            .Produces<UserDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", UpdateUser)
            .WithName("UpdateUser")
            .WithSummary("Update a user")
            .Produces<UserDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> GetUsers(
        IDispatcher dispatcher,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUsersQuery(limit);
        var result = await dispatcher.QueryAsync(query, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetUserById(
        Guid id,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUserByIdQuery(id);
        var result = await dispatcher.QueryAsync(query, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> CreateUser(
        CreateUserRequest request,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateUserCommand(request.Email, request.DisplayName, request.Password);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToCreatedResult($"/api/users/{result.Value?.Id}");
    }

    private static async Task<IResult> UpdateUser(
        Guid id,
        UpdateUserRequest request,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateUserCommand(id, request.DisplayName, request.Email);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToHttpResult();
    }
}
