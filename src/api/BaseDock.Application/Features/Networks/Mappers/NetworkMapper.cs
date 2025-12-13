namespace BaseDock.Application.Features.Networks.Mappers;

using BaseDock.Application.Features.Networks.DTOs;
using BaseDock.Domain.Entities;

public static class NetworkMapper
{
    public static NetworkDto ToDto(this Network entity)
    {
        return new NetworkDto(
            entity.Id,
            entity.EnvironmentId,
            entity.Name,
            entity.Driver,
            entity.DriverOpts,
            entity.IpamDriver,
            entity.IpamConfig,
            entity.Internal,
            entity.Attachable,
            entity.Labels,
            entity.External,
            entity.ExternalName,
            entity.CreatedAt);
    }

    public static IEnumerable<NetworkDto> ToDtos(this IEnumerable<Network> entities)
    {
        return entities.Select(e => e.ToDto());
    }
}
