namespace BaseDock.Application.Features.Secrets.Mappers;

using BaseDock.Application.Features.Secrets.DTOs;
using BaseDock.Domain.Entities;

public static class SecretMapper
{
    public static SecretDto ToDto(this Secret entity)
    {
        return new SecretDto(
            entity.Id,
            entity.EnvironmentId,
            entity.Name,
            !string.IsNullOrEmpty(entity.Content),
            entity.FilePath,
            entity.External,
            entity.ExternalName,
            entity.CreatedAt);
    }

    public static IEnumerable<SecretDto> ToDtos(this IEnumerable<Secret> entities)
    {
        return entities.Select(e => e.ToDto());
    }
}
