namespace BaseDock.Application.Features.Environments.Mappers;

using BaseDock.Application.Features.Environments.DTOs;
using BaseDock.Domain.Entities;

public static class EnvironmentMapper
{
    public static EnvironmentDto ToDto(this Environment entity)
    {
        return new EnvironmentDto(
            entity.Id,
            entity.Name,
            entity.Slug,
            entity.Description,
            entity.ProjectId,
            entity.IsDefault,
            entity.CreatedAt,
            entity.Services.Count,
            entity.Volumes.Count,
            entity.Networks.Count);
    }

    public static IEnumerable<EnvironmentDto> ToDtos(this IEnumerable<Environment> entities)
    {
        return entities.Select(e => e.ToDto());
    }

    public static EnvironmentDetailDto ToDetailDto(this Environment entity)
    {
        var services = entity.Services.Select(s => new ServiceSummaryDto(
            s.Id,
            s.Name,
            s.Slug,
            s.Image,
            s.DeploymentStatus.ToString(),
            s.DependsOn));

        var volumes = entity.Volumes.Select(v => new VolumeSummaryDto(
            v.Id,
            v.Name,
            v.Driver ?? "local"));

        var networks = entity.Networks.Select(n => new NetworkSummaryDto(
            n.Id,
            n.Name,
            n.Driver ?? "bridge"));

        var configs = entity.Configs.Select(c => new ConfigSummaryDto(
            c.Id,
            c.Name));

        var secrets = entity.Secrets.Select(s => new SecretSummaryDto(
            s.Id,
            s.Name));

        return new EnvironmentDetailDto(
            entity.Id,
            entity.Name,
            entity.Slug,
            entity.Description,
            entity.ProjectId,
            entity.IsDefault,
            entity.CreatedAt,
            services,
            volumes,
            networks,
            configs,
            secrets);
    }
}
