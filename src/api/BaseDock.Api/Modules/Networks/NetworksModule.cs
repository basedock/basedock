namespace BaseDock.Api.Modules.Networks;

using System.Security.Claims;
using BaseDock.Api.Extensions;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Networks.Commands.CreateNetwork;
using BaseDock.Application.Features.Networks.Commands.DeleteNetwork;
using BaseDock.Application.Features.Networks.DTOs;
using BaseDock.Application.Features.Networks.Queries.GetNetworks;
using Carter;

public class NetworksModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects/{projectSlug}/environments/{envSlug}/networks")
            .WithTags("Networks");

        group.MapGet("/", GetNetworks)
            .WithName("GetNetworks")
            .WithSummary("Get all networks in an environment")
            .Produces<IEnumerable<NetworkDto>>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPost("/", CreateNetwork)
            .WithName("CreateNetwork")
            .WithSummary("Create a new network")
            .Produces<NetworkDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapDelete("/{networkId:guid}", DeleteNetwork)
            .WithName("DeleteNetwork")
            .WithSummary("Delete a network")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    private static async Task<IResult> GetNetworks(
        string projectSlug,
        string envSlug,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        var query = new GetNetworksQuery(projectSlug, envSlug, userId);
        var result = await dispatcher.QueryAsync(query, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> CreateNetwork(
        string projectSlug,
        string envSlug,
        CreateNetworkRequest request,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        var command = new CreateNetworkCommand(
            projectSlug,
            envSlug,
            request.Name,
            request.Driver,
            request.DriverOpts,
            request.IpamDriver,
            request.IpamConfig,
            request.Internal,
            request.Attachable,
            request.Labels,
            request.External,
            request.ExternalName,
            userId);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToCreatedResult($"/api/projects/{projectSlug}/environments/{envSlug}/networks/{result.Value?.Id}");
    }

    private static async Task<IResult> DeleteNetwork(
        string projectSlug,
        string envSlug,
        Guid networkId,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        var command = new DeleteNetworkCommand(projectSlug, envSlug, networkId, userId);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToHttpResult();
    }
}
