namespace BaseDock.Api.Modules.Volumes;

using System.Security.Claims;
using BaseDock.Api.Extensions;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Volumes.Commands.CreateVolume;
using BaseDock.Application.Features.Volumes.Commands.DeleteVolume;
using BaseDock.Application.Features.Volumes.DTOs;
using BaseDock.Application.Features.Volumes.Queries.GetVolumes;
using Carter;

public class VolumesModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects/{projectSlug}/environments/{envSlug}/volumes")
            .WithTags("Volumes");

        group.MapGet("/", GetVolumes)
            .WithName("GetVolumes")
            .WithSummary("Get all volumes in an environment")
            .Produces<IEnumerable<VolumeDto>>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPost("/", CreateVolume)
            .WithName("CreateVolume")
            .WithSummary("Create a new volume")
            .Produces<VolumeDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapDelete("/{volumeId:guid}", DeleteVolume)
            .WithName("DeleteVolume")
            .WithSummary("Delete a volume")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    private static async Task<IResult> GetVolumes(
        string projectSlug,
        string envSlug,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        var query = new GetVolumesQuery(projectSlug, envSlug, userId);
        var result = await dispatcher.QueryAsync(query, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> CreateVolume(
        string projectSlug,
        string envSlug,
        CreateVolumeRequest request,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        var command = new CreateVolumeCommand(
            projectSlug,
            envSlug,
            request.Name,
            request.Driver,
            request.DriverOpts,
            request.Labels,
            request.External,
            request.ExternalName,
            userId);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToCreatedResult($"/api/projects/{projectSlug}/environments/{envSlug}/volumes/{result.Value?.Id}");
    }

    private static async Task<IResult> DeleteVolume(
        string projectSlug,
        string envSlug,
        Guid volumeId,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        var command = new DeleteVolumeCommand(projectSlug, envSlug, volumeId, userId);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToHttpResult();
    }
}
