namespace BaseDock.Application.Features.Templates.Commands.ApplyTemplate;

using System.Text.Json;
using System.Text.RegularExpressions;
using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Templates.DTOs;
using BaseDock.Domain.Entities;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed partial class ApplyTemplateCommandHandler(IApplicationDbContext db, TimeProvider dateTime)
    : ICommandHandler<ApplyTemplateCommand, Result<ApplyTemplateResult>>
{
    private readonly TimeProvider _dateTime = dateTime;

    public async Task<Result<ApplyTemplateResult>> HandleAsync(
        ApplyTemplateCommand command,
        CancellationToken cancellationToken = default)
    {
        // Find project and verify membership
        var project = await db.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Slug == command.ProjectSlug, cancellationToken);

        if (project is null)
        {
            return Result.Failure<ApplyTemplateResult>(
                Error.NotFound("Project.NotFound", $"Project with slug '{command.ProjectSlug}' not found."));
        }

        if (!project.Members.Any(m => m.UserId == command.UserId))
        {
            return Result.Failure<ApplyTemplateResult>(
                Error.Forbidden("You are not a member of this project."));
        }

        // Find environment
        var environment = await db.Environments
            .FirstOrDefaultAsync(e => e.ProjectId == project.Id && e.Slug == command.EnvironmentSlug, cancellationToken);

        if (environment is null)
        {
            return Result.Failure<ApplyTemplateResult>(
                Error.NotFound("Environment.NotFound", $"Environment with slug '{command.EnvironmentSlug}' not found."));
        }

        // Get template
        var template = TemplateDefinitions.GetById(command.TemplateId);
        if (template is null)
        {
            return Result.Failure<ApplyTemplateResult>(
                Error.NotFound("Template.NotFound", $"Template '{command.TemplateId}' not found."));
        }

        // Validate required parameters
        foreach (var param in template.Parameters.Where(p => p.Required))
        {
            if (!command.Parameters.ContainsKey(param.Key) || string.IsNullOrWhiteSpace(command.Parameters[param.Key]))
            {
                // Use default value if available
                if (!string.IsNullOrWhiteSpace(param.DefaultValue))
                {
                    command.Parameters[param.Key] = param.DefaultValue;
                }
                else
                {
                    return Result.Failure<ApplyTemplateResult>(
                        Error.Validation("Template.MissingParameter", $"Required parameter '{param.Name}' ({param.Key}) is missing."));
                }
            }
        }

        // Apply default values for optional parameters
        foreach (var param in template.Parameters.Where(p => !p.Required && !string.IsNullOrWhiteSpace(p.DefaultValue)))
        {
            if (!command.Parameters.ContainsKey(param.Key))
            {
                command.Parameters[param.Key] = param.DefaultValue!;
            }
        }

        var createdServices = new List<CreatedServiceDto>();
        var now = _dateTime.GetUtcNow();

        // Create volumes first
        foreach (var volumeName in template.Volumes)
        {
            var existingVolume = await db.Volumes
                .AnyAsync(v => v.EnvironmentId == environment.Id && v.Name == volumeName, cancellationToken);

            if (!existingVolume)
            {
                var volume = Volume.Create(
                    environment.Id,
                    volumeName,
                    "local",
                    null,
                    null,
                    false,
                    null,
                    now);
                db.Volumes.Add(volume);
            }
        }

        // Create services
        foreach (var serviceTemplate in template.ServiceTemplates)
        {
            var serviceName = ReplaceParameters(serviceTemplate.Name, command.Parameters);
            var slug = await GenerateUniqueSlugAsync(environment.Id, serviceName, cancellationToken);

            // Replace parameters in all template values
            var envVars = serviceTemplate.EnvironmentVariables != null
                ? JsonSerializer.Serialize(
                    serviceTemplate.EnvironmentVariables.ToDictionary(
                        kvp => kvp.Key,
                        kvp => ReplaceParameters(kvp.Value, command.Parameters)))
                : null;

            var ports = serviceTemplate.Ports != null
                ? ReplaceParameters(serviceTemplate.Ports, command.Parameters)
                : null;

            var volumes = serviceTemplate.Volumes != null
                ? ReplaceParameters(serviceTemplate.Volumes, command.Parameters)
                : null;

            var dependsOn = serviceTemplate.DependsOn != null
                ? ReplaceParameters(serviceTemplate.DependsOn, command.Parameters)
                : null;

            var healthcheckTest = serviceTemplate.HealthcheckTest?
                .Select(t => ReplaceParameters(t, command.Parameters))
                .ToArray();

            var service = Service.Create(
                environment.Id,
                serviceName,
                slug,
                $"Created from {template.Name} template",
                serviceTemplate.Image,
                now);

            service.Update(
                serviceName,
                $"Created from {template.Name} template",
                serviceTemplate.Image,
                null, // buildContext
                null, // buildDockerfile
                null, // buildArgs
                serviceTemplate.Command,
                null, // entrypoint
                null, // workingDir
                null, // user
                ports,
                null, // expose
                null, // hostname
                null, // domainname
                null, // dns
                null, // extraHosts
                envVars,
                null, // envFile
                volumes,
                null, // tmpfs
                dependsOn,
                null, // links
                healthcheckTest,
                serviceTemplate.HealthcheckInterval,
                serviceTemplate.HealthcheckTimeout,
                serviceTemplate.HealthcheckRetries,
                serviceTemplate.HealthcheckStartPeriod,
                false, // healthcheckDisabled
                null, // cpuLimit
                null, // memoryLimit
                null, // cpuReservation
                null, // memoryReservation
                serviceTemplate.Restart,
                null, // stopGracePeriod
                null, // stopSignal
                1, // replicas
                null, // labels
                now);

            db.Services.Add(service);
            createdServices.Add(new CreatedServiceDto(service.Id, service.Name, service.Slug));
        }

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(new ApplyTemplateResult(createdServices));
    }

    private static string ReplaceParameters(string template, Dictionary<string, string> parameters)
    {
        var result = template;
        foreach (var param in parameters)
        {
            result = result.Replace($"${{{param.Key}}}", param.Value);
        }
        return result;
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
