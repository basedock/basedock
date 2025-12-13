namespace BaseDock.Application.Features.Services.Commands.DeleteService;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Domain.Primitives;

public sealed record DeleteServiceCommand(
    string ProjectSlug,
    string EnvironmentSlug,
    Guid ServiceId,
    Guid UserId) : ICommand<Result>;
