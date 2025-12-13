namespace BaseDock.Application.Features.Volumes.Mappers;

using BaseDock.Application.Features.Volumes.DTOs;
using BaseDock.Domain.Entities;

public static class VolumeMapper
{
    public static VolumeDto ToDto(this Volume entity)
    {
        return new VolumeDto(
            entity.Id,
            entity.EnvironmentId,
            entity.Name,
            entity.Driver,
            entity.DriverOpts,
            entity.Labels,
            entity.External,
            entity.ExternalName,
            entity.CreatedAt);
    }

    public static IEnumerable<VolumeDto> ToDtos(this IEnumerable<Volume> entities)
    {
        return entities.Select(e => e.ToDto());
    }
}
