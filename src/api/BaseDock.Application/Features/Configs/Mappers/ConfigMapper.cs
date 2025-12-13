namespace BaseDock.Application.Features.Configs.Mappers;

using BaseDock.Application.Features.Configs.DTOs;
using BaseDock.Domain.Entities;

public static class ConfigMapper
{
    public static ConfigDto ToDto(this Config entity)
    {
        return new ConfigDto(
            entity.Id,
            entity.EnvironmentId,
            entity.Name,
            entity.Content,
            entity.FilePath,
            entity.External,
            entity.ExternalName,
            entity.CreatedAt);
    }

    public static IEnumerable<ConfigDto> ToDtos(this IEnumerable<Config> entities)
    {
        return entities.Select(e => e.ToDto());
    }
}
