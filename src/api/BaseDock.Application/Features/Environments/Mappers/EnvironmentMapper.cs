namespace BaseDock.Application.Features.Environments.Mappers;

using BaseDock.Application.Features.Environments.DTOs;
using BaseDock.Domain.Entities;
using BaseDock.Domain.Entities.Resources;

public static class EnvironmentMapper
{
    public static EnvironmentDto ToDto(this Environment entity)
    {
        var resourceCount =
            entity.DockerImageResources.Count +
            entity.DockerfileResources.Count +
            entity.DockerComposeResources.Count +
            entity.PostgreSQLResources.Count +
            entity.RedisResources.Count +
            entity.PreMadeAppResources.Count;

        return new EnvironmentDto(
            entity.Id,
            entity.Name,
            entity.Slug,
            entity.Description,
            entity.ProjectId,
            entity.IsDefault,
            entity.CreatedAt,
            entity.Variables.Count,
            resourceCount);
    }

    public static IEnumerable<EnvironmentDto> ToDtos(this IEnumerable<Environment> entities)
    {
        return entities.Select(e => e.ToDto());
    }

    public static EnvironmentDetailDto ToDetailDto(this Environment entity)
    {
        var variables = entity.Variables.Select(v => v.ToDto());
        var resources = GetAllResources(entity);

        return new EnvironmentDetailDto(
            entity.Id,
            entity.Name,
            entity.Slug,
            entity.Description,
            entity.ProjectId,
            entity.IsDefault,
            entity.CreatedAt,
            variables,
            resources);
    }

    public static EnvironmentVariableDto ToDto(this EnvironmentVariable entity)
    {
        return new EnvironmentVariableDto(
            entity.Id,
            entity.Key,
            entity.IsSecret ? "••••••••" : entity.Value,
            entity.IsSecret,
            entity.CreatedAt);
    }

    private static IEnumerable<ResourceSummaryDto> GetAllResources(Environment entity)
    {
        var resources = new List<ResourceSummaryDto>();

        // Docker Image Resources
        resources.AddRange(entity.DockerImageResources.Select(r => new ResourceSummaryDto(
            r.Id,
            r.Name,
            "DockerImage",
            r.DeploymentStatus.ToString())));

        // Dockerfile Resources
        resources.AddRange(entity.DockerfileResources.Select(r => new ResourceSummaryDto(
            r.Id,
            r.Name,
            "Dockerfile",
            r.DeploymentStatus.ToString())));

        // Docker Compose Resources
        resources.AddRange(entity.DockerComposeResources.Select(r => new ResourceSummaryDto(
            r.Id,
            r.Name,
            "DockerCompose",
            r.DeploymentStatus.ToString())));

        // PostgreSQL Resources
        resources.AddRange(entity.PostgreSQLResources.Select(r => new ResourceSummaryDto(
            r.Id,
            r.Name,
            "PostgreSQL",
            r.DeploymentStatus.ToString())));

        // Redis Resources
        resources.AddRange(entity.RedisResources.Select(r => new ResourceSummaryDto(
            r.Id,
            r.Name,
            "Redis",
            r.DeploymentStatus.ToString())));

        // Pre-Made App Resources
        resources.AddRange(entity.PreMadeAppResources.Select(r => new ResourceSummaryDto(
            r.Id,
            r.Name,
            "PreMadeApp",
            r.DeploymentStatus.ToString())));

        return resources;
    }
}
