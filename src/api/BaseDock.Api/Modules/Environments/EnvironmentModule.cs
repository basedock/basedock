namespace BaseDock.Api.Modules.Environments;

using System.Security.Claims;
using BaseDock.Api.Extensions;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Environments.Commands.CreateEnvironment;
using BaseDock.Application.Features.Environments.DTOs;
using BaseDock.Application.Features.Environments.Queries.GetEnvironmentBySlug;
using BaseDock.Application.Features.Environments.Queries.GetEnvironmentsByProject;
using Carter;

public class EnvironmentModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects/{projectSlug}/environments")
            .WithTags("Environments");

        group.MapGet("/", GetEnvironments)
            .WithName("GetEnvironments")
            .WithSummary("Get all environments for a project")
            .Produces<IEnumerable<EnvironmentDto>>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/{envSlug}", GetEnvironment)
            .WithName("GetEnvironmentBySlug")
            .WithSummary("Get environment details by slug")
            .Produces<EnvironmentDetailDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPost("/", CreateEnvironment)
            .WithName("CreateEnvironment")
            .WithSummary("Create a new environment (Admin only)")
            .Produces<EnvironmentDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
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

    private static async Task<IResult> GetEnvironments(
        string projectSlug,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        var query = new GetEnvironmentsByProjectQuery(projectSlug, userId);
        var result = await dispatcher.QueryAsync(query, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetEnvironment(
        string projectSlug,
        string envSlug,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        var query = new GetEnvironmentBySlugQuery(projectSlug, envSlug, userId);
        var result = await dispatcher.QueryAsync(query, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> CreateEnvironment(
        string projectSlug,
        CreateEnvironmentRequest request,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        if (!IsAdmin(user))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Error.Forbidden",
                detail: "Only administrators can create environments.");
        }

        var userId = GetUserId(user);
        var command = new CreateEnvironmentCommand(
            projectSlug,
            request.Name,
            request.Description,
            userId);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToCreatedResult($"/api/projects/{projectSlug}/environments/{result.Value?.Slug}");
    }
}
