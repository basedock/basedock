namespace BaseDock.Application.Features.Networks.Commands.CreateNetwork;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Networks.DTOs;
using BaseDock.Application.Features.Networks.Mappers;
using BaseDock.Domain.Entities;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class CreateNetworkCommandHandler(IApplicationDbContext db, TimeProvider dateTime)
    : ICommandHandler<CreateNetworkCommand, Result<NetworkDto>>
{
    private readonly TimeProvider _dateTime = dateTime;

    public async Task<Result<NetworkDto>> HandleAsync(
        CreateNetworkCommand command,
        CancellationToken cancellationToken = default)
    {
        var project = await db.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Slug == command.ProjectSlug, cancellationToken);

        if (project is null)
        {
            return Result.Failure<NetworkDto>(
                Error.NotFound("Project.NotFound", $"Project with slug '{command.ProjectSlug}' not found."));
        }

        if (!project.Members.Any(m => m.UserId == command.UserId))
        {
            return Result.Failure<NetworkDto>(
                Error.Forbidden("You are not a member of this project."));
        }

        var environment = await db.Environments
            .FirstOrDefaultAsync(e => e.ProjectId == project.Id && e.Slug == command.EnvironmentSlug, cancellationToken);

        if (environment is null)
        {
            return Result.Failure<NetworkDto>(
                Error.NotFound("Environment.NotFound", $"Environment with slug '{command.EnvironmentSlug}' not found."));
        }

        var nameExists = await db.Networks
            .AnyAsync(n => n.EnvironmentId == environment.Id && n.Name == command.Name, cancellationToken);

        if (nameExists)
        {
            return Result.Failure<NetworkDto>(
                Error.Conflict("Network.NameExists", $"A network with name '{command.Name}' already exists in this environment."));
        }

        var network = Network.Create(
            environment.Id,
            command.Name,
            command.Driver,
            command.DriverOpts,
            command.IpamDriver,
            command.IpamConfig,
            command.Internal,
            command.Attachable,
            command.Labels,
            command.External,
            command.ExternalName,
            _dateTime.GetUtcNow());

        db.Networks.Add(network);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(network.ToDto());
    }
}
