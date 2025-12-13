namespace BaseDock.Api.Modules.Services;

using System.Security.Claims;
using BaseDock.Api.Extensions;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Services.Commands.CreateService;
using BaseDock.Application.Features.Services.Commands.DeleteService;
using BaseDock.Application.Features.Services.Commands.UpdateService;
using BaseDock.Application.Features.Services.DTOs;
using BaseDock.Application.Features.Services.Queries.GetServiceById;
using BaseDock.Application.Features.Services.Queries.GetServices;
using Carter;

public class ServicesModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects/{projectSlug}/environments/{envSlug}/services")
            .WithTags("Services");

        group.MapGet("/", GetServices)
            .WithName("GetServices")
            .WithSummary("Get all services in an environment")
            .Produces<IEnumerable<ServiceDto>>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/{serviceId:guid}", GetService)
            .WithName("GetServiceById")
            .WithSummary("Get service details by ID")
            .Produces<ServiceDetailDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPost("/", CreateService)
            .WithName("CreateService")
            .WithSummary("Create a new service")
            .Produces<ServiceDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPut("/{serviceId:guid}", UpdateService)
            .WithName("UpdateService")
            .WithSummary("Update a service")
            .Produces<ServiceDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapDelete("/{serviceId:guid}", DeleteService)
            .WithName("DeleteService")
            .WithSummary("Delete a service")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    private static async Task<IResult> GetServices(
        string projectSlug,
        string envSlug,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        var query = new GetServicesQuery(projectSlug, envSlug, userId);
        var result = await dispatcher.QueryAsync(query, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetService(
        string projectSlug,
        string envSlug,
        Guid serviceId,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        var query = new GetServiceByIdQuery(projectSlug, envSlug, serviceId, userId);
        var result = await dispatcher.QueryAsync(query, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> CreateService(
        string projectSlug,
        string envSlug,
        CreateServiceRequest request,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        var command = new CreateServiceCommand(
            projectSlug,
            envSlug,
            request.Name,
            request.Description,
            request.Image,
            request.BuildContext,
            request.BuildDockerfile,
            request.BuildArgs,
            request.Command,
            request.Entrypoint,
            request.WorkingDir,
            request.User,
            request.Ports,
            request.Expose,
            request.Hostname,
            request.Domainname,
            request.Dns,
            request.ExtraHosts,
            request.EnvironmentVariables,
            request.EnvFile,
            request.Volumes,
            request.Tmpfs,
            request.DependsOn,
            request.Links,
            request.HealthcheckTest,
            request.HealthcheckInterval,
            request.HealthcheckTimeout,
            request.HealthcheckRetries,
            request.HealthcheckStartPeriod,
            request.HealthcheckDisabled,
            request.CpuLimit,
            request.MemoryLimit,
            request.CpuReservation,
            request.MemoryReservation,
            request.Restart,
            request.StopGracePeriod,
            request.StopSignal,
            request.Replicas,
            request.Labels,
            userId);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToCreatedResult($"/api/projects/{projectSlug}/environments/{envSlug}/services/{result.Value?.Id}");
    }

    private static async Task<IResult> UpdateService(
        string projectSlug,
        string envSlug,
        Guid serviceId,
        UpdateServiceRequest request,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        var command = new UpdateServiceCommand(
            projectSlug,
            envSlug,
            serviceId,
            request.Name,
            request.Description,
            request.Image,
            request.BuildContext,
            request.BuildDockerfile,
            request.BuildArgs,
            request.Command,
            request.Entrypoint,
            request.WorkingDir,
            request.User,
            request.Ports,
            request.Expose,
            request.Hostname,
            request.Domainname,
            request.Dns,
            request.ExtraHosts,
            request.EnvironmentVariables,
            request.EnvFile,
            request.Volumes,
            request.Tmpfs,
            request.DependsOn,
            request.Links,
            request.HealthcheckTest,
            request.HealthcheckInterval,
            request.HealthcheckTimeout,
            request.HealthcheckRetries,
            request.HealthcheckStartPeriod,
            request.HealthcheckDisabled,
            request.CpuLimit,
            request.MemoryLimit,
            request.CpuReservation,
            request.MemoryReservation,
            request.Restart,
            request.StopGracePeriod,
            request.StopSignal,
            request.Replicas,
            request.Labels,
            userId);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> DeleteService(
        string projectSlug,
        string envSlug,
        Guid serviceId,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        var command = new DeleteServiceCommand(projectSlug, envSlug, serviceId, userId);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToHttpResult();
    }
}
