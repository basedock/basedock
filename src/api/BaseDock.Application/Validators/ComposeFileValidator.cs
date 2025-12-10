namespace BaseDock.Application.Validators;

using System.Text.RegularExpressions;
using BaseDock.Domain.Primitives;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

public static partial class ComposeFileValidator
{
    private const int MaxFileSizeBytes = 65536; // 64KB

    private static readonly string[] DangerousBindMounts =
    [
        "/",
        "/etc",
        "/var",
        "/root",
        "/home",
        "/sys",
        "/proc",
        "/dev",
        "/boot",
        "/bin",
        "/sbin",
        "/usr"
    ];

    private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
        .IgnoreUnmatchedProperties()
        .Build();

    public static Result Validate(string composeContent)
    {
        if (string.IsNullOrWhiteSpace(composeContent))
        {
            return Result.Failure(Error.DockerComposeInvalid("Compose file content cannot be empty."));
        }

        // 1. Size limit check
        if (composeContent.Length > MaxFileSizeBytes)
        {
            return Result.Failure(Error.DockerComposeInvalid(
                $"Compose file exceeds maximum size ({MaxFileSizeBytes / 1024}KB)."));
        }

        // 2. Basic YAML syntax validation
        try
        {
            YamlDeserializer.Deserialize<object>(composeContent);
        }
        catch (YamlException ex)
        {
            return Result.Failure(Error.DockerComposeInvalid($"Invalid YAML syntax: {ex.Message}"));
        }

        // 3. Check for privileged mode
        if (PrivilegedModeRegex().IsMatch(composeContent))
        {
            return Result.Failure(Error.DockerComposeInvalid(
                "Privileged mode is not allowed for security reasons."));
        }

        // 4. Check for dangerous volume mounts
        foreach (var mount in DangerousBindMounts)
        {
            // Match patterns like "- /etc:" or '- "/etc:'
            if (DangerousVolumeRegex(mount).IsMatch(composeContent))
            {
                return Result.Failure(Error.DockerComposeInvalid(
                    $"Dangerous volume mount detected: {mount}. Host system directories cannot be mounted."));
            }
        }

        // 5. Check for network_mode: host
        if (HostNetworkModeRegex().IsMatch(composeContent))
        {
            return Result.Failure(Error.DockerComposeInvalid(
                "Host network mode is not allowed for security reasons."));
        }

        // 6. Check for pid: host
        if (HostPidModeRegex().IsMatch(composeContent))
        {
            return Result.Failure(Error.DockerComposeInvalid(
                "Host PID mode is not allowed for security reasons."));
        }

        // 7. Check for cap_add with dangerous capabilities
        if (DangerousCapabilitiesRegex().IsMatch(composeContent))
        {
            return Result.Failure(Error.DockerComposeInvalid(
                "Adding SYS_ADMIN or ALL capabilities is not allowed for security reasons."));
        }

        return Result.Success();
    }

    [GeneratedRegex(@"privileged\s*:\s*true", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex PrivilegedModeRegex();

    [GeneratedRegex(@"network_mode\s*:\s*[""']?host[""']?", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex HostNetworkModeRegex();

    [GeneratedRegex(@"pid\s*:\s*[""']?host[""']?", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex HostPidModeRegex();

    [GeneratedRegex(@"cap_add\s*:[\s\S]*?(SYS_ADMIN|ALL)", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex DangerousCapabilitiesRegex();

    private static Regex DangerousVolumeRegex(string mount)
    {
        // Escape the mount path for regex and match volume patterns
        var escaped = Regex.Escape(mount);
        return new Regex($@"-\s*[""']?{escaped}[""']?\s*:", RegexOptions.IgnoreCase | RegexOptions.Multiline);
    }
}
