namespace BaseDock.Application.Features.Configs.Commands.CreateConfig;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Configs.DTOs;
using BaseDock.Domain.Primitives;

public sealed record CreateConfigCommand(
    string ProjectSlug,
    string EnvironmentSlug,
    string Name,
    string? Content,
    string? FilePath,
    bool External,
    string? ExternalName,
    Guid UserId) : ICommand<Result<ConfigDto>>;
