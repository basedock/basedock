namespace BaseDock.Application.Features.Docker.Queries.GetProjectLogs;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Domain.Primitives;

public sealed record GetProjectLogsQuery(
    Guid ProjectId,
    string? ServiceName,
    int TailLines = 100) : IQuery<Result<string>>;
