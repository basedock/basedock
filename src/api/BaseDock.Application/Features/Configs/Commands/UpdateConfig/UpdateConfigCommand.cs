namespace BaseDock.Application.Features.Configs.Commands.UpdateConfig;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Configs.DTOs;
using BaseDock.Domain.Primitives;

public sealed record UpdateConfigCommand(
    string ProjectSlug,
    string EnvironmentSlug,
    Guid ConfigId,
    string? Content,
    string? FilePath,
    bool External,
    string? ExternalName,
    Guid UserId) : ICommand<Result<ConfigDto>>;
