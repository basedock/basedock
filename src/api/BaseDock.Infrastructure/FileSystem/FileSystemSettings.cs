namespace BaseDock.Infrastructure.FileSystem;

public class FileSystemSettings
{
    public const string SectionName = "FileSystem";

    public string BasePath { get; init; } = "/app/data/projects";
}
