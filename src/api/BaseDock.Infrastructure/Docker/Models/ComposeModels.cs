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

    [YamlMember(Alias = "ports")]
    public List<string>? Ports { get; set; }

    [YamlMember(Alias = "environment")]
    public Dictionary<string, string>? Environment { get; set; }

    [YamlMember(Alias = "volumes")]
    public List<string>? Volumes { get; set; }

    [YamlMember(Alias = "networks")]
    public List<string>? Networks { get; set; }

    [YamlMember(Alias = "depends_on")]
    public List<string>? DependsOn { get; set; }

    [YamlMember(Alias = "labels")]
    public Dictionary<string, string>? Labels { get; set; }

    [YamlMember(Alias = "deploy")]
    public ComposeDeploy? Deploy { get; set; }
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

    [YamlMember(Alias = "external")]
    public bool? External { get; set; }
}

public class ComposeVolume
{
    [YamlMember(Alias = "driver")]
    public string? Driver { get; set; }

    [YamlMember(Alias = "external")]
    public bool? External { get; set; }
}

public class ComposeDeploy
{
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
