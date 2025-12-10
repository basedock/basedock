namespace BaseDock.Api.Modules.Docker;

using System.Security.Claims;
using BaseDock.Api.Extensions;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Docker.Commands.DeployProject;
using BaseDock.Application.Features.Docker.Commands.RemoveProject;
using BaseDock.Application.Features.Docker.Commands.RestartProject;
using BaseDock.Application.Features.Docker.Commands.StopProject;
using BaseDock.Application.Features.Docker.Commands.UpdateComposeFile;
using BaseDock.Application.Features.Docker.DTOs;
using BaseDock.Application.Features.Docker.Queries.GetProjectLogs;
using BaseDock.Application.Features.Docker.Queries.GetProjectStatus;
using BaseDock.Application.Features.Projects.DTOs;
using Carter;

public class DockerModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects/{projectId:guid}/docker")
            .WithTags("Docker");

        group.MapPut("/compose", UpdateComposeFile)
            .WithName("UpdateComposeFile")
            .WithSummary("Update project compose file (Admin only)")
            .Produces<ProjectDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPost("/deploy", DeployProject)
            .WithName("DeployProject")
            .WithSummary("Deploy project containers (Admin only)")
            .Produces<DeploymentStatusDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPost("/stop", StopProject)
            .WithName("StopProject")
            .WithSummary("Stop project containers (Admin only)")
            .Produces<DeploymentStatusDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPost("/restart", RestartProject)
            .WithName("RestartProject")
            .WithSummary("Restart project containers (Admin only)")
            .Produces<DeploymentStatusDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapDelete("/", RemoveContainers)
            .WithName("RemoveProjectContainers")
            .WithSummary("Remove project containers and deployment files (Admin only)")
            .Produces<DeploymentStatusDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/status", GetStatus)
            .WithName("GetProjectDockerStatus")
            .WithSummary("Get project deployment status")
            .Produces<DeploymentStatusDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/logs", GetLogs)
            .WithName("GetProjectLogs")
            .WithSummary("Get project container logs")
            .Produces<string>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static bool IsAdmin(ClaimsPrincipal user)
    {
        var isAdminClaim = user.FindFirst("isAdmin")?.Value;
        return bool.TryParse(isAdminClaim, out var isAdmin) && isAdmin;
    }

    private static async Task<IResult> UpdateComposeFile(
        Guid projectId,
        UpdateComposeFileRequest request,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        if (!IsAdmin(user))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Error.Forbidden",
                detail: "Only administrators can update compose files.");
        }

        var command = new UpdateComposeFileCommand(projectId, request.ComposeFileContent);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> DeployProject(
        Guid projectId,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        if (!IsAdmin(user))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Error.Forbidden",
                detail: "Only administrators can deploy projects.");
        }

        var command = new DeployProjectCommand(projectId);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> StopProject(
        Guid projectId,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        if (!IsAdmin(user))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Error.Forbidden",
                detail: "Only administrators can stop projects.");
        }

        var command = new StopProjectCommand(projectId);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> RestartProject(
        Guid projectId,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        if (!IsAdmin(user))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Error.Forbidden",
                detail: "Only administrators can restart projects.");
        }

        var command = new RestartProjectCommand(projectId);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> RemoveContainers(
        Guid projectId,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        if (!IsAdmin(user))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Error.Forbidden",
                detail: "Only administrators can remove project containers.");
        }

        var command = new RemoveProjectCommand(projectId);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetStatus(
        Guid projectId,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProjectStatusQuery(projectId);
        var result = await dispatcher.QueryAsync(query, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetLogs(
        Guid projectId,
        IDispatcher dispatcher,
        string? serviceName = null,
        int tailLines = 100,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProjectLogsQuery(projectId, serviceName, tailLines);
        var result = await dispatcher.QueryAsync(query, cancellationToken);
        return result.ToHttpResult();
    }
}
