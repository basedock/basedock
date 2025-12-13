namespace BaseDock.Application.Features.Environments.DTOs;

public sealed record EnvironmentDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    Guid ProjectId,
    bool IsDefault,
    DateTimeOffset CreatedAt,
    int ServiceCount,
    int VolumeCount,
    int NetworkCount);

public sealed record EnvironmentDetailDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    Guid ProjectId,
    bool IsDefault,
    DateTimeOffset CreatedAt,
    IEnumerable<ServiceSummaryDto> Services,
    IEnumerable<VolumeSummaryDto> Volumes,
    IEnumerable<NetworkSummaryDto> Networks,
    IEnumerable<ConfigSummaryDto> Configs,
    IEnumerable<SecretSummaryDto> Secrets);

public sealed record ServiceSummaryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Image,
    string Status);

public sealed record VolumeSummaryDto(
    Guid Id,
    string Name,
    string Driver);

public sealed record NetworkSummaryDto(
    Guid Id,
    string Name,
    string Driver);

public sealed record ConfigSummaryDto(
    Guid Id,
    string Name);

public sealed record SecretSummaryDto(
    Guid Id,
    string Name);

public sealed record CreateEnvironmentRequest(
    string Name,
    string? Description);
