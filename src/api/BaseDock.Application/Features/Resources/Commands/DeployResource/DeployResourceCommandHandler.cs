namespace BaseDock.Application.Features.Resources.Commands.DeployResource;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Docker;
using BaseDock.Application.Abstractions.FileSystem;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Domain.Enums;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class DeployResourceCommandHandler(
    IApplicationDbContext db,
    IDockerComposeService composeService,
    IComposeGeneratorService composeGenerator,
    IProjectFileService fileService,
    TimeProvider dateTime)
    : ICommandHandler<DeployResourceCommand>
{
    public async Task<Result> HandleAsync(
        DeployResourceCommand command,
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

        // Ensure compose file exists
        var projectName = composeGenerator.GetProjectName(command.ProjectSlug, command.EnvSlug);
        var composePath = fileService.GetComposeFilePath(projectName);

        if (string.IsNullOrEmpty(environment.ComposeFilePath) || !File.Exists(composePath))
        {
            // Generate compose file if it doesn't exist
            var generateResult = await composeGenerator.GenerateComposeFileAsync(
                environment.Id,
                command.ProjectSlug,
                cancellationToken);

            if (generateResult.IsFailure)
            {
                return Result.Failure(generateResult.Error);
            }

            environment.SetComposeFilePath(generateResult.Value);
            await db.SaveChangesAsync(cancellationToken);
            composePath = generateResult.Value;
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
                pgResource.SetDeploymentStatus(DeploymentStatus.Deploying);
                break;

            case "redis":
                var redisResource = await db.RedisResources
                    .FirstOrDefaultAsync(r => r.Id == command.ResourceId && r.EnvironmentId == environment.Id, cancellationToken);
                if (redisResource is null)
                    return Result.Failure(Error.NotFound("Resource.NotFound", "Redis resource not found."));
                resourceSlug = redisResource.Slug;
                redisResource.SetDeploymentStatus(DeploymentStatus.Deploying);
                break;

            case "dockerimage":
                var imageResource = await db.DockerImageResources
                    .FirstOrDefaultAsync(r => r.Id == command.ResourceId && r.EnvironmentId == environment.Id, cancellationToken);
                if (imageResource is null)
                    return Result.Failure(Error.NotFound("Resource.NotFound", "Docker image resource not found."));
                resourceSlug = imageResource.Slug;
                imageResource.SetDeploymentStatus(DeploymentStatus.Deploying);
                break;

            case "dockerfile":
                var dockerfileResource = await db.DockerfileResources
                    .FirstOrDefaultAsync(r => r.Id == command.ResourceId && r.EnvironmentId == environment.Id, cancellationToken);
                if (dockerfileResource is null)
                    return Result.Failure(Error.NotFound("Resource.NotFound", "Dockerfile resource not found."));
                resourceSlug = dockerfileResource.Slug;
                dockerfileResource.SetDeploymentStatus(DeploymentStatus.Deploying);
                break;

            case "premadeapp":
                var premadeResource = await db.PreMadeAppResources
                    .FirstOrDefaultAsync(r => r.Id == command.ResourceId && r.EnvironmentId == environment.Id, cancellationToken);
                if (premadeResource is null)
                    return Result.Failure(Error.NotFound("Resource.NotFound", "Pre-made app resource not found."));
                resourceSlug = premadeResource.Slug;
                premadeResource.SetDeploymentStatus(DeploymentStatus.Deploying);
                break;

            default:
                return Result.Failure(Error.Validation("Resource.InvalidType", $"Unknown resource type: {command.ResourceType}"));
        }

        await db.SaveChangesAsync(cancellationToken);

        // Deploy the service
        var serviceName = composeGenerator.GetServiceName(command.ProjectSlug, command.EnvSlug, resourceSlug);
        var deployResult = await composeService.DeployServiceAsync(
            projectName,
            composePath,
            serviceName,
            cancellationToken);

        // Update deployment status
        var now = dateTime.GetUtcNow();
        switch (command.ResourceType.ToLowerInvariant())
        {
            case "postgresql":
                var pg = await db.PostgreSQLResources.FindAsync([command.ResourceId], cancellationToken);
                if (pg != null)
                {
                    if (deployResult.IsSuccess)
                        pg.SetDeploymentStatus(DeploymentStatus.Running, now);
                    else
                        pg.SetDeploymentError(deployResult.Error.Message);
                }
                break;

            case "redis":
                var redis = await db.RedisResources.FindAsync([command.ResourceId], cancellationToken);
                if (redis != null)
                {
                    if (deployResult.IsSuccess)
                        redis.SetDeploymentStatus(DeploymentStatus.Running, now);
                    else
                        redis.SetDeploymentError(deployResult.Error.Message);
                }
                break;

            case "dockerimage":
                var image = await db.DockerImageResources.FindAsync([command.ResourceId], cancellationToken);
                if (image != null)
                {
                    if (deployResult.IsSuccess)
                        image.SetDeploymentStatus(DeploymentStatus.Running, now);
                    else
                        image.SetDeploymentError(deployResult.Error.Message);
                }
                break;

            case "dockerfile":
                var dockerfile = await db.DockerfileResources.FindAsync([command.ResourceId], cancellationToken);
                if (dockerfile != null)
                {
                    if (deployResult.IsSuccess)
                        dockerfile.SetDeploymentStatus(DeploymentStatus.Running, now);
                    else
                        dockerfile.SetDeploymentError(deployResult.Error.Message);
                }
                break;

            case "premadeapp":
                var premade = await db.PreMadeAppResources.FindAsync([command.ResourceId], cancellationToken);
                if (premade != null)
                {
                    if (deployResult.IsSuccess)
                        premade.SetDeploymentStatus(DeploymentStatus.Running, now);
                    else
                        premade.SetDeploymentError(deployResult.Error.Message);
                }
                break;
        }

        await db.SaveChangesAsync(cancellationToken);

        return deployResult;
    }
}
