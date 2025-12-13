namespace BaseDock.Infrastructure.Docker.Models;

using YamlDotNet.Serialization;

public class ComposeFile
{
    [YamlMember(Alias = "version")]
    public string Version { get; set; } = "3.8";

    [YamlMember(Alias = "services")]
    public Dictionary<string, ComposeService> Services { get; set; } = new();

    [YamlMember(Alias = "networks")]
    public Dictionary<string, ComposeNetwork>? Networks { get; set; }

    [YamlMember(Alias = "volumes")]
    public Dictionary<string, ComposeVolume>? Volumes { get; set; }

    [YamlMember(Alias = "configs")]
    public Dictionary<string, ComposeConfig>? Configs { get; set; }

    [YamlMember(Alias = "secrets")]
    public Dictionary<string, ComposeSecret>? Secrets { get; set; }
}

public class ComposeService
{
    [YamlMember(Alias = "image")]
    public string? Image { get; set; }

    [YamlMember(Alias = "build")]
    public ComposeBuild? Build { get; set; }

    [YamlMember(Alias = "container_name")]
    public string? ContainerName { get; set; }

    [YamlMember(Alias = "restart")]
    public string? Restart { get; set; }

    [YamlMember(Alias = "command")]
    public string? Command { get; set; }

    [YamlMember(Alias = "entrypoint")]
    public string? Entrypoint { get; set; }

    [YamlMember(Alias = "working_dir")]
    public string? WorkingDir { get; set; }

    [YamlMember(Alias = "user")]
    public string? User { get; set; }

    [YamlMember(Alias = "hostname")]
    public string? Hostname { get; set; }

    [YamlMember(Alias = "domainname")]
    public string? Domainname { get; set; }

    [YamlMember(Alias = "ports")]
    public List<string>? Ports { get; set; }

    [YamlMember(Alias = "expose")]
    public List<string>? Expose { get; set; }

    [YamlMember(Alias = "dns")]
    public List<string>? Dns { get; set; }

    [YamlMember(Alias = "extra_hosts")]
    public List<string>? ExtraHosts { get; set; }

    [YamlMember(Alias = "environment")]
    public Dictionary<string, string>? Environment { get; set; }

    [YamlMember(Alias = "env_file")]
    public List<string>? EnvFile { get; set; }

    [YamlMember(Alias = "volumes")]
    public List<string>? Volumes { get; set; }

    [YamlMember(Alias = "tmpfs")]
    public List<string>? Tmpfs { get; set; }

    [YamlMember(Alias = "networks")]
    public List<string>? Networks { get; set; }

    [YamlMember(Alias = "depends_on")]
    public List<string>? DependsOn { get; set; }

    [YamlMember(Alias = "healthcheck")]
    public ComposeHealthcheck? Healthcheck { get; set; }

    [YamlMember(Alias = "labels")]
    public Dictionary<string, string>? Labels { get; set; }

    [YamlMember(Alias = "deploy")]
    public ComposeDeploy? Deploy { get; set; }

    [YamlMember(Alias = "stop_grace_period")]
    public string? StopGracePeriod { get; set; }

    [YamlMember(Alias = "stop_signal")]
    public string? StopSignal { get; set; }

    [YamlMember(Alias = "configs")]
    public List<ComposeConfigReference>? Configs { get; set; }

    [YamlMember(Alias = "secrets")]
    public List<ComposeSecretReference>? Secrets { get; set; }
}

public class ComposeBuild
{
    [YamlMember(Alias = "context")]
    public string Context { get; set; } = null!;

    [YamlMember(Alias = "dockerfile")]
    public string? Dockerfile { get; set; }

    [YamlMember(Alias = "args")]
    public Dictionary<string, string>? Args { get; set; }
}

public class ComposeNetwork
{
    [YamlMember(Alias = "driver")]
    public string? Driver { get; set; }

    [YamlMember(Alias = "driver_opts")]
    public Dictionary<string, string>? DriverOpts { get; set; }

    [YamlMember(Alias = "internal")]
    public bool? Internal { get; set; }

    [YamlMember(Alias = "attachable")]
    public bool? Attachable { get; set; }

    [YamlMember(Alias = "labels")]
    public Dictionary<string, string>? Labels { get; set; }

    [YamlMember(Alias = "external")]
    public bool? External { get; set; }

    [YamlMember(Alias = "name")]
    public string? Name { get; set; }
}

public class ComposeVolume
{
    [YamlMember(Alias = "driver")]
    public string? Driver { get; set; }

    [YamlMember(Alias = "driver_opts")]
    public Dictionary<string, string>? DriverOpts { get; set; }

    [YamlMember(Alias = "labels")]
    public Dictionary<string, string>? Labels { get; set; }

    [YamlMember(Alias = "external")]
    public bool? External { get; set; }

    [YamlMember(Alias = "name")]
    public string? Name { get; set; }
}

public class ComposeConfig
{
    [YamlMember(Alias = "file")]
    public string? File { get; set; }

    [YamlMember(Alias = "external")]
    public bool? External { get; set; }

    [YamlMember(Alias = "name")]
    public string? Name { get; set; }
}

public class ComposeSecret
{
    [YamlMember(Alias = "file")]
    public string? File { get; set; }

    [YamlMember(Alias = "external")]
    public bool? External { get; set; }

    [YamlMember(Alias = "name")]
    public string? Name { get; set; }
}

public class ComposeConfigReference
{
    [YamlMember(Alias = "source")]
    public string Source { get; set; } = null!;

    [YamlMember(Alias = "target")]
    public string? Target { get; set; }

    [YamlMember(Alias = "uid")]
    public string? Uid { get; set; }

    [YamlMember(Alias = "gid")]
    public string? Gid { get; set; }

    [YamlMember(Alias = "mode")]
    public string? Mode { get; set; }
}

public class ComposeSecretReference
{
    [YamlMember(Alias = "source")]
    public string Source { get; set; } = null!;

    [YamlMember(Alias = "target")]
    public string? Target { get; set; }

    [YamlMember(Alias = "uid")]
    public string? Uid { get; set; }

    [YamlMember(Alias = "gid")]
    public string? Gid { get; set; }

    [YamlMember(Alias = "mode")]
    public string? Mode { get; set; }
}

public class ComposeHealthcheck
{
    [YamlMember(Alias = "test")]
    public List<string>? Test { get; set; }

    [YamlMember(Alias = "interval")]
    public string? Interval { get; set; }

    [YamlMember(Alias = "timeout")]
    public string? Timeout { get; set; }

    [YamlMember(Alias = "retries")]
    public int? Retries { get; set; }

    [YamlMember(Alias = "start_period")]
    public string? StartPeriod { get; set; }
}

public class ComposeDeploy
{
    [YamlMember(Alias = "replicas")]
    public int? Replicas { get; set; }

    [YamlMember(Alias = "resources")]
    public ComposeResources? Resources { get; set; }
}

public class ComposeResources
{
    [YamlMember(Alias = "limits")]
    public ResourceSpec? Limits { get; set; }

    [YamlMember(Alias = "reservations")]
    public ResourceSpec? Reservations { get; set; }
}

public class ResourceSpec
{
    [YamlMember(Alias = "cpus")]
    public string? Cpus { get; set; }

    [YamlMember(Alias = "memory")]
    public string? Memory { get; set; }
}
