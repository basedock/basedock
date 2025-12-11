namespace BaseDock.Application.Features.Environments.DTOs;

public sealed record EnvironmentDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    Guid ProjectId,
    bool IsDefault,
    DateTime CreatedAt,
    int VariableCount,
    int ResourceCount);

public sealed record EnvironmentDetailDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    Guid ProjectId,
    bool IsDefault,
    DateTime CreatedAt,
    IEnumerable<EnvironmentVariableDto> Variables,
    IEnumerable<ResourceSummaryDto> Resources);

public sealed record EnvironmentVariableDto(
    Guid Id,
    string Key,
    string Value,
    bool IsSecret,
    DateTime CreatedAt);

public sealed record ResourceSummaryDto(
    Guid Id,
    string Name,
    string Type,
    string Status);

public sealed record CreateEnvironmentRequest(
    string Name,
    string? Description);
