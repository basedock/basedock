namespace BaseDock.Application.Abstractions.FileSystem;

using BaseDock.Domain.Primitives;

public interface IProjectFileService
{
    Task<Result<string>> CreateProjectDirectoryAsync(string projectName, CancellationToken ct = default);

    Task<Result> WriteComposeFileAsync(string projectName, string content, CancellationToken ct = default);

    Task<Result<string>> ReadComposeFileAsync(string projectName, CancellationToken ct = default);

    Task<Result> DeleteProjectDirectoryAsync(string projectName, CancellationToken ct = default);

    string GetProjectPath(string projectName);

    string GetComposeFilePath(string projectName);

    bool ProjectDirectoryExists(string projectName);
}
