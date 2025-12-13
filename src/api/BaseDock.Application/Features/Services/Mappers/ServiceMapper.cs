namespace BaseDock.Application.Features.Services.Mappers;

using BaseDock.Application.Features.Services.DTOs;
using BaseDock.Domain.Entities;

public static class ServiceMapper
{
    public static ServiceDto ToDto(this Service entity)
    {
        return new ServiceDto(
            entity.Id,
            entity.EnvironmentId,
            entity.Name,
            entity.Slug,
            entity.Description,
            entity.Image,
            entity.BuildContext,
            entity.BuildDockerfile,
            entity.DeploymentStatus.ToString(),
            entity.LastDeployedAt,
            entity.LastError,
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    public static IEnumerable<ServiceDto> ToDtos(this IEnumerable<Service> entities)
    {
        return entities.Select(e => e.ToDto());
    }

    public static ServiceDetailDto ToDetailDto(this Service entity)
    {
        var networks = entity.ServiceNetworks.Select(sn => new ServiceNetworkDto(
            sn.NetworkId,
            sn.Network.Name,
            sn.Aliases,
            sn.Ipv4Address,
            sn.Ipv6Address));

        var configs = entity.ServiceConfigs.Select(sc => new ServiceConfigDto(
            sc.ConfigId,
            sc.Config.Name,
            sc.Target,
            sc.Uid,
            sc.Gid,
            sc.Mode));

        var secrets = entity.ServiceSecrets.Select(ss => new ServiceSecretDto(
            ss.SecretId,
            ss.Secret.Name,
            ss.Target,
            ss.Uid,
            ss.Gid,
            ss.Mode));

        return new ServiceDetailDto(
            entity.Id,
            entity.EnvironmentId,
            entity.Name,
            entity.Slug,
            entity.Description,
            entity.Image,
            entity.BuildContext,
            entity.BuildDockerfile,
            entity.BuildArgs,
            entity.Command,
            entity.Entrypoint,
            entity.WorkingDir,
            entity.User,
            entity.Ports,
            entity.Expose,
            entity.Hostname,
            entity.Domainname,
            entity.Dns,
            entity.ExtraHosts,
            entity.EnvironmentVariables,
            entity.EnvFile,
            entity.Volumes,
            entity.Tmpfs,
            entity.DependsOn,
            entity.Links,
            entity.HealthcheckTest,
            entity.HealthcheckInterval,
            entity.HealthcheckTimeout,
            entity.HealthcheckRetries,
            entity.HealthcheckStartPeriod,
            entity.HealthcheckDisabled,
            entity.CpuLimit,
            entity.MemoryLimit,
            entity.CpuReservation,
            entity.MemoryReservation,
            entity.Restart,
            entity.StopGracePeriod,
            entity.StopSignal,
            entity.Replicas,
            entity.Labels,
            entity.DeploymentStatus.ToString(),
            entity.LastDeployedAt,
            entity.LastError,
            entity.CreatedAt,
            entity.UpdatedAt,
            networks,
            configs,
            secrets);
    }
}
