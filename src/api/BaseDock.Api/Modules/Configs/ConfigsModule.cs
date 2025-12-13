namespace BaseDock.Api.Modules.Configs;

using System.Security.Claims;
using BaseDock.Api.Extensions;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Configs.Commands.CreateConfig;
using BaseDock.Application.Features.Configs.Commands.DeleteConfig;
using BaseDock.Application.Features.Configs.Commands.UpdateConfig;
using BaseDock.Application.Features.Configs.DTOs;
using BaseDock.Application.Features.Configs.Queries.GetConfigs;
using Carter;

public class ConfigsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects/{projectSlug}/environments/{envSlug}/configs")
            .WithTags("Configs");

        group.MapGet("/", GetConfigs)
            .WithName("GetConfigs")
            .WithSummary("Get all configs in an environment")
            .Produces<IEnumerable<ConfigDto>>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPost("/", CreateConfig)
            .WithName("CreateConfig")
            .WithSummary("Create a new config")
            .Produces<ConfigDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPut("/{configId:guid}", UpdateConfig)
            .WithName("UpdateConfig")
            .WithSummary("Update a config")
            .Produces<ConfigDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapDelete("/{configId:guid}", DeleteConfig)
            .WithName("DeleteConfig")
            .WithSummary("Delete a config")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    private static async Task<IResult> GetConfigs(
        string projectSlug,
        string envSlug,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        var query = new GetConfigsQuery(projectSlug, envSlug, userId);
        var result = await dispatcher.QueryAsync(query, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> CreateConfig(
        string projectSlug,
        string envSlug,
        CreateConfigRequest request,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        var command = new CreateConfigCommand(
            projectSlug,
            envSlug,
            request.Name,
            request.Content,
            request.FilePath,
            request.External,
            request.ExternalName,
            userId);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToCreatedResult($"/api/projects/{projectSlug}/environments/{envSlug}/configs/{result.Value?.Id}");
    }

    private static async Task<IResult> UpdateConfig(
        string projectSlug,
        string envSlug,
        Guid configId,
        UpdateConfigRequest request,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        var command = new UpdateConfigCommand(
            projectSlug,
            envSlug,
            configId,
            request.Content,
            request.FilePath,
            request.External,
            request.ExternalName,
            userId);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> DeleteConfig(
        string projectSlug,
        string envSlug,
        Guid configId,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        var command = new DeleteConfigCommand(projectSlug, envSlug, configId, userId);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToHttpResult();
    }
}
