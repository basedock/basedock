namespace BaseDock.Application.Features.Services.Commands.UpdateService;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Services.DTOs;
using BaseDock.Application.Features.Services.Mappers;
using BaseDock.Application.Features.Services.Validators;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class UpdateServiceCommandHandler(IApplicationDbContext db, TimeProvider dateTime)
    : ICommandHandler<UpdateServiceCommand, Result<ServiceDto>>
{
    private readonly TimeProvider _dateTime = dateTime;

    public async Task<Result<ServiceDto>> HandleAsync(
        UpdateServiceCommand command,
        CancellationToken cancellationToken = default)
    {
        // Find project and verify membership
        var project = await db.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Slug == command.ProjectSlug, cancellationToken);

        if (project is null)
        {
            return Result.Failure<ServiceDto>(
                Error.NotFound("Project.NotFound", $"Project with slug '{command.ProjectSlug}' not found."));
        }

        if (!project.Members.Any(m => m.UserId == command.UserId))
        {
            return Result.Failure<ServiceDto>(
                Error.Forbidden("You are not a member of this project."));
        }

        // Find environment
        var environment = await db.Environments
            .FirstOrDefaultAsync(e => e.ProjectId == project.Id && e.Slug == command.EnvironmentSlug, cancellationToken);

        if (environment is null)
        {
            return Result.Failure<ServiceDto>(
                Error.NotFound("Environment.NotFound", $"Environment with slug '{command.EnvironmentSlug}' not found."));
        }

        // Find service
        var service = await db.Services
            .FirstOrDefaultAsync(s => s.Id == command.ServiceId && s.EnvironmentId == environment.Id, cancellationToken);

        if (service is null)
        {
            return Result.Failure<ServiceDto>(
                Error.NotFound("Service.NotFound", "Service not found."));
        }

        // Validate circular dependencies
        if (!string.IsNullOrWhiteSpace(command.DependsOn))
        {
            var allServices = await db.Services
                .AsNoTracking()
                .Where(s => s.EnvironmentId == environment.Id)
                .ToListAsync(cancellationToken);

            var circularError = CircularDependencyValidator.Validate(
                command.Name,
                command.DependsOn,
                allServices,
                service.Id); // Exclude current service from graph

            if (circularError is not null)
            {
                return Result.Failure<ServiceDto>(circularError);
            }
        }

        // Check name uniqueness if name changed
        if (service.Name != command.Name)
        {
            var nameExists = await db.Services
                .AnyAsync(s => s.EnvironmentId == environment.Id && s.Name == command.Name && s.Id != service.Id, cancellationToken);

            if (nameExists)
            {
                return Result.Failure<ServiceDto>(
                    Error.Conflict("Service.NameExists", $"A service with name '{command.Name}' already exists in this environment."));
            }
        }

        // Validate: Either image or build context is required
        if (string.IsNullOrWhiteSpace(command.Image) && string.IsNullOrWhiteSpace(command.BuildContext))
        {
            return Result.Failure<ServiceDto>(
                Error.Validation("Service.ImageOrBuildRequired", "Either an image or a build context is required."));
        }

        service.Update(
            command.Name,
            command.Description,
            command.Image,
            command.BuildContext,
            command.BuildDockerfile,
            command.BuildArgs,
            command.Command,
            command.Entrypoint,
            command.WorkingDir,
            command.User,
            command.Ports,
            command.Expose,
            command.Hostname,
            command.Domainname,
            command.Dns,
            command.ExtraHosts,
            command.EnvironmentVariables,
            command.EnvFile,
            command.Volumes,
            command.Tmpfs,
            command.DependsOn,
            command.Links,
            command.HealthcheckTest,
            command.HealthcheckInterval,
            command.HealthcheckTimeout,
            command.HealthcheckRetries,
            command.HealthcheckStartPeriod,
            command.HealthcheckDisabled,
            command.CpuLimit,
            command.MemoryLimit,
            command.CpuReservation,
            command.MemoryReservation,
            command.Restart,
            command.StopGracePeriod,
            command.StopSignal,
            command.Replicas,
            command.Labels,
            _dateTime.GetUtcNow());

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(service.ToDto());
    }
}
