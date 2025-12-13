namespace BaseDock.Api.Modules.Resources;

using System.Security.Claims;
using BaseDock.Api.Extensions;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Resources.Commands.CreatePostgreSQLResource;
using BaseDock.Application.Features.Resources.Commands.DeployResource;
using BaseDock.Application.Features.Resources.Commands.StopResource;
using BaseDock.Application.Features.Resources.DTOs;
using Carter;

public class ResourceModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects/{projectSlug}/environments/{envSlug}/resources")
            .WithTags("Resources");

        // PostgreSQL
        group.MapPost("/postgresql", CreatePostgreSQLResource)
            .WithName("CreatePostgreSQLResource")
            .WithSummary("Create a new PostgreSQL resource")
            .Produces<PostgreSQLResourceDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        // Deploy/Stop actions
        group.MapPost("/{resourceId:guid}/deploy", DeployResource)
            .WithName("DeployResource")
            .WithSummary("Deploy a resource")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/{resourceId:guid}/stop", StopResource)
            .WithName("StopResource")
            .WithSummary("Stop a resource")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    private static async Task<IResult> CreatePostgreSQLResource(
        string projectSlug,
        string envSlug,
        CreatePostgreSQLResourceRequest request,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        var command = new CreatePostgreSQLResourceCommand(
            projectSlug,
            envSlug,
            request.Name,
            request.Description,
            request.DatabaseName,
            request.Username,
            request.Password,
            request.Version,
            request.Port,
            userId);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToCreatedResult($"/api/projects/{projectSlug}/environments/{envSlug}/resources/{result.Value?.Slug}");
    }

    private static async Task<IResult> DeployResource(
        string projectSlug,
        string envSlug,
        Guid resourceId,
        DeployResourceRequest request,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        var command = new DeployResourceCommand(
            projectSlug,
            envSlug,
            resourceId,
            request.ResourceType,
            userId);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> StopResource(
        string projectSlug,
        string envSlug,
        Guid resourceId,
        StopResourceRequest request,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        var command = new StopResourceCommand(
            projectSlug,
            envSlug,
            resourceId,
            request.ResourceType,
            userId);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToHttpResult();
    }
}
