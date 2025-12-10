namespace BaseDock.Infrastructure.FileSystem;

using System.Text.RegularExpressions;
using BaseDock.Application.Abstractions.FileSystem;
using BaseDock.Domain.Primitives;
using Microsoft.Extensions.Options;

public partial class ProjectFileService : IProjectFileService
{
    private readonly string _basePath;
    private const string ComposeFileName = "compose.yml";

    public ProjectFileService(IOptions<FileSystemSettings> settings)
    {
        var basePath = settings.Value.BasePath;

        // Convert to absolute path if relative
        _basePath = Path.IsPathRooted(basePath)
            ? basePath
            : Path.GetFullPath(basePath);
    }

    public string GetProjectPath(string projectName)
    {
        var sanitized = SanitizeProjectName(projectName);
        return Path.Combine(_basePath, sanitized);
    }

    public string GetComposeFilePath(string projectName)
    {
        return Path.Combine(GetProjectPath(projectName), ComposeFileName);
    }

    public bool ProjectDirectoryExists(string projectName)
    {
        return Directory.Exists(GetProjectPath(projectName));
    }

    public Task<Result<string>> CreateProjectDirectoryAsync(string projectName, CancellationToken ct = default)
    {
        try
        {
            var path = GetProjectPath(projectName);

            if (Directory.Exists(path))
            {
                return Task.FromResult(Result.Failure<string>(
                    Error.Conflict("Project.DirectoryExists", $"Project directory already exists: {path}")));
            }

            // Ensure base directory exists
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }

            Directory.CreateDirectory(path);
            return Task.FromResult(Result.Success(path));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result.Failure<string>(
                Error.Validation("FileSystem.Error", $"Failed to create project directory: {ex.Message}")));
        }
    }

    public async Task<Result> WriteComposeFileAsync(string projectName, string content, CancellationToken ct = default)
    {
        try
        {
            var projectPath = GetProjectPath(projectName);

            // Create directory if it doesn't exist
            if (!Directory.Exists(projectPath))
            {
                Directory.CreateDirectory(projectPath);
            }

            var filePath = GetComposeFilePath(projectName);
            await File.WriteAllTextAsync(filePath, content, ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(
                Error.Validation("FileSystem.Error", $"Failed to write compose file: {ex.Message}"));
        }
    }

    public async Task<Result<string>> ReadComposeFileAsync(string projectName, CancellationToken ct = default)
    {
        try
        {
            var filePath = GetComposeFilePath(projectName);

            if (!File.Exists(filePath))
            {
                return Result.Failure<string>(
                    Error.NotFound("ComposeFile", projectName));
            }

            var content = await File.ReadAllTextAsync(filePath, ct);
            return Result.Success(content);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(
                Error.Validation("FileSystem.Error", $"Failed to read compose file: {ex.Message}"));
        }
    }

    public Task<Result> DeleteProjectDirectoryAsync(string projectName, CancellationToken ct = default)
    {
        try
        {
            var path = GetProjectPath(projectName);

            if (!Directory.Exists(path))
            {
                return Task.FromResult(Result.Success());
            }

            Directory.Delete(path, recursive: true);
            return Task.FromResult(Result.Success());
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result.Failure(
                Error.Validation("FileSystem.Error", $"Failed to delete project directory: {ex.Message}")));
        }
    }

    private static string SanitizeProjectName(string name)
    {
        // Convert to lowercase and replace invalid characters with dashes
        var sanitized = SanitizeRegex().Replace(name.ToLowerInvariant(), "-");

        // Remove consecutive dashes
        sanitized = ConsecutiveDashRegex().Replace(sanitized, "-");

        // Trim dashes from start and end
        return sanitized.Trim('-');
    }

    [GeneratedRegex(@"[^a-z0-9_-]")]
    private static partial Regex SanitizeRegex();

    [GeneratedRegex(@"-+")]
    private static partial Regex ConsecutiveDashRegex();
}
