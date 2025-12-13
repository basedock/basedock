namespace BaseDock.Application.Features.Resources.DTOs;

public sealed record ResourceDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string Type,
    string Status,
    string ServiceName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastDeployedAt);

public sealed record PostgreSQLResourceDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string Version,
    int Port,
    string DatabaseName,
    string Username,
    string Status,
    string ServiceName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastDeployedAt);

public sealed record RedisResourceDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string Version,
    int Port,
    bool PersistenceEnabled,
    string Status,
    string ServiceName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastDeployedAt);

public sealed record DockerImageResourceDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string Image,
    string Tag,
    string Status,
    string ServiceName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastDeployedAt);

public sealed record PreMadeAppResourceDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string TemplateType,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastDeployedAt);

// Request DTOs
public sealed record CreatePostgreSQLResourceRequest(
    string Name,
    string? Description,
    string DatabaseName,
    string Username,
    string Password,
    string Version = "16",
    int Port = 5432);

public sealed record CreateRedisResourceRequest(
    string Name,
    string? Description,
    string? Password,
    string Version = "7",
    int Port = 6379,
    bool PersistenceEnabled = true);

public sealed record CreateDockerImageResourceRequest(
    string Name,
    string Image,
    string? Description = null,
    string Tag = "latest",
    string? Ports = null,
    string? EnvironmentVariables = null,
    string? Volumes = null,
    string RestartPolicy = "unless-stopped");

public sealed record CreatePreMadeAppResourceRequest(
    string Name,
    string? Description,
    string TemplateType,
    string? Configuration);

public sealed record DeployResourceRequest(string ResourceType);

public sealed record StopResourceRequest(string ResourceType);
