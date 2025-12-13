namespace BaseDock.Application.Abstractions.Docker;

using BaseDock.Domain.Primitives;

public interface IComposeGeneratorService
{
    Task<Result<string>> GenerateComposeFileAsync(
        Guid environmentId,
        string projectSlug,
        CancellationToken ct = default);

    string GetServiceName(string projectSlug, string envSlug, string resourceSlug);

    string GetNetworkName(string projectSlug, string envSlug);

    string GetProjectName(string projectSlug, string envSlug);
}
