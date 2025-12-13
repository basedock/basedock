namespace BaseDock.Application.Features.Resources.Commands.StopResource;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Docker;
using BaseDock.Application.Abstractions.FileSystem;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Domain.Enums;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class StopResourceCommandHandler(
    IApplicationDbContext db,
    IDockerComposeService composeService,
    IComposeGeneratorService composeGenerator,
    IProjectFileService fileService)
    : ICommandHandler<StopResourceCommand>
{
    public async Task<Result> HandleAsync(
        StopResourceCommand command,
        CancellationToken cancellationToken = default)
    {
        // Find project and verify membership
        var project = await db.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Slug == command.ProjectSlug, cancellationToken);

        if (project is null)
        {
            return Result.Failure(
                Error.NotFound("Project.NotFound", $"Project with slug '{command.ProjectSlug}' not found."));
        }

        if (!project.Members.Any(m => m.UserId == command.UserId))
        {
            return Result.Failure(Error.Forbidden("You are not a member of this project."));
        }

        // Find environment
        var environment = await db.Environments
            .FirstOrDefaultAsync(e =>
                e.ProjectId == project.Id &&
                e.Slug == command.EnvSlug,
                cancellationToken);

        if (environment is null)
        {
            return Result.Failure(
                Error.NotFound("Environment.NotFound", $"Environment with slug '{command.EnvSlug}' not found."));
        }

        // Check compose file exists
        var projectName = composeGenerator.GetProjectName(command.ProjectSlug, command.EnvSlug);
        var composePath = fileService.GetComposeFilePath(projectName);

        if (string.IsNullOrEmpty(environment.ComposeFilePath) || !File.Exists(composePath))
        {
            return Result.Failure(Error.Validation("Compose.NotFound", "Compose file not found. Deploy the resource first."));
        }

        // Find the resource and get its service name
        string? resourceSlug = null;

        switch (command.ResourceType.ToLowerInvariant())
        {
            case "postgresql":
                var pgResource = await db.PostgreSQLResources
                    .FirstOrDefaultAsync(r => r.Id == command.ResourceId && r.EnvironmentId == environment.Id, cancellationToken);
                if (pgResource is null)
                    return Result.Failure(Error.NotFound("Resource.NotFound", "PostgreSQL resource not found."));
                resourceSlug = pgResource.Slug;
                break;

            case "redis":
                var redisResource = await db.RedisResources
                    .FirstOrDefaultAsync(r => r.Id == command.ResourceId && r.EnvironmentId == environment.Id, cancellationToken);
                if (redisResource is null)
                    return Result.Failure(Error.NotFound("Resource.NotFound", "Redis resource not found."));
                resourceSlug = redisResource.Slug;
                break;

            case "dockerimage":
                var imageResource = await db.DockerImageResources
                    .FirstOrDefaultAsync(r => r.Id == command.ResourceId && r.EnvironmentId == environment.Id, cancellationToken);
                if (imageResource is null)
                    return Result.Failure(Error.NotFound("Resource.NotFound", "Docker image resource not found."));
                resourceSlug = imageResource.Slug;
                break;

            case "dockerfile":
                var dockerfileResource = await db.DockerfileResources
                    .FirstOrDefaultAsync(r => r.Id == command.ResourceId && r.EnvironmentId == environment.Id, cancellationToken);
                if (dockerfileResource is null)
                    return Result.Failure(Error.NotFound("Resource.NotFound", "Dockerfile resource not found."));
                resourceSlug = dockerfileResource.Slug;
                break;

            case "premadeapp":
                var premadeResource = await db.PreMadeAppResources
                    .FirstOrDefaultAsync(r => r.Id == command.ResourceId && r.EnvironmentId == environment.Id, cancellationToken);
                if (premadeResource is null)
                    return Result.Failure(Error.NotFound("Resource.NotFound", "Pre-made app resource not found."));
                resourceSlug = premadeResource.Slug;
                break;

            default:
                return Result.Failure(Error.Validation("Resource.InvalidType", $"Unknown resource type: {command.ResourceType}"));
        }

        // Stop the service
        var serviceName = composeGenerator.GetServiceName(command.ProjectSlug, command.EnvSlug, resourceSlug);
        var stopResult = await composeService.StopServiceAsync(
            projectName,
            composePath,
            serviceName,
            cancellationToken);

        // Update deployment status
        if (stopResult.IsSuccess)
        {
            switch (command.ResourceType.ToLowerInvariant())
            {
                case "postgresql":
                    var pg = await db.PostgreSQLResources.FindAsync([command.ResourceId], cancellationToken);
                    pg?.SetDeploymentStatus(DeploymentStatus.Stopped);
                    break;

                case "redis":
                    var redis = await db.RedisResources.FindAsync([command.ResourceId], cancellationToken);
                    redis?.SetDeploymentStatus(DeploymentStatus.Stopped);
                    break;

                case "dockerimage":
                    var image = await db.DockerImageResources.FindAsync([command.ResourceId], cancellationToken);
                    image?.SetDeploymentStatus(DeploymentStatus.Stopped);
                    break;

                case "dockerfile":
                    var dockerfile = await db.DockerfileResources.FindAsync([command.ResourceId], cancellationToken);
                    dockerfile?.SetDeploymentStatus(DeploymentStatus.Stopped);
                    break;

                case "premadeapp":
                    var premade = await db.PreMadeAppResources.FindAsync([command.ResourceId], cancellationToken);
                    premade?.SetDeploymentStatus(DeploymentStatus.Stopped);
                    break;
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        return stopResult;
    }
}
