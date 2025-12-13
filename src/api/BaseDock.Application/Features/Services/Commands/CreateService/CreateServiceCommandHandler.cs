namespace BaseDock.Application.Features.Services.Commands.CreateService;

using System.Text.RegularExpressions;
using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Services.DTOs;
using BaseDock.Application.Features.Services.Mappers;
using BaseDock.Application.Features.Services.Validators;
using BaseDock.Domain.Entities;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed partial class CreateServiceCommandHandler(IApplicationDbContext db, TimeProvider dateTime)
    : ICommandHandler<CreateServiceCommand, Result<ServiceDto>>
{
    private readonly TimeProvider _dateTime = dateTime;

    public async Task<Result<ServiceDto>> HandleAsync(
        CreateServiceCommand command,
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
                allServices);

            if (circularError is not null)
            {
                return Result.Failure<ServiceDto>(circularError);
            }
        }

        // Check name uniqueness within environment
        var nameExists = await db.Services
            .AnyAsync(s => s.EnvironmentId == environment.Id && s.Name == command.Name, cancellationToken);

        if (nameExists)
        {
            return Result.Failure<ServiceDto>(
                Error.Conflict("Service.NameExists", $"A service with name '{command.Name}' already exists in this environment."));
        }

        // Validate: Either image or build context is required
        if (string.IsNullOrWhiteSpace(command.Image) && string.IsNullOrWhiteSpace(command.BuildContext))
        {
            return Result.Failure<ServiceDto>(
                Error.Validation("Service.ImageOrBuildRequired", "Either an image or a build context is required."));
        }

        // Generate unique slug within environment
        var slug = await GenerateUniqueSlugAsync(environment.Id, command.Name, cancellationToken);

        var service = Service.Create(
            environment.Id,
            command.Name,
            slug,
            command.Description,
            command.Image,
            _dateTime.GetUtcNow());

        // Update with all the details
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

        db.Services.Add(service);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(service.ToDto());
    }

    private async Task<string> GenerateUniqueSlugAsync(Guid environmentId, string name, CancellationToken cancellationToken)
    {
        var baseSlug = GenerateSlug(name);
        var slug = baseSlug;
        var counter = 1;

        while (await db.Services.AnyAsync(s => s.EnvironmentId == environmentId && s.Slug == slug, cancellationToken))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;

            if (counter > 100)
            {
                slug = $"{baseSlug}-{Guid.NewGuid().ToString()[..8]}";
                break;
            }
        }

        return slug;
    }

    private static string GenerateSlug(string text)
    {
        var slug = text.ToLowerInvariant();
        slug = slug.Replace(' ', '-');
        slug = SlugRegex().Replace(slug, "");
        slug = MultipleHyphensRegex().Replace(slug, "-");
        slug = slug.Trim('-');
        return slug;
    }

    [GeneratedRegex("[^a-z0-9-]")]
    private static partial Regex SlugRegex();

    [GeneratedRegex("-+")]
    private static partial Regex MultipleHyphensRegex();
}
