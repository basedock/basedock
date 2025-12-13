namespace BaseDock.Application.Features.Services.Queries.GetServiceById;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Services.DTOs;
using BaseDock.Application.Features.Services.Mappers;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class GetServiceByIdQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetServiceByIdQuery, Result<ServiceDetailDto>>
{
    public async Task<Result<ServiceDetailDto>> HandleAsync(
        GetServiceByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        // Find project and verify membership
        var project = await db.Projects
            .AsNoTracking()
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Slug == query.ProjectSlug, cancellationToken);

        if (project is null)
        {
            return Result.Failure<ServiceDetailDto>(
                Error.NotFound("Project.NotFound", $"Project with slug '{query.ProjectSlug}' not found."));
        }

        if (!project.Members.Any(m => m.UserId == query.UserId))
        {
            return Result.Failure<ServiceDetailDto>(
                Error.Forbidden("You are not a member of this project."));
        }

        // Find environment
        var environment = await db.Environments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ProjectId == project.Id && e.Slug == query.EnvironmentSlug, cancellationToken);

        if (environment is null)
        {
            return Result.Failure<ServiceDetailDto>(
                Error.NotFound("Environment.NotFound", $"Environment with slug '{query.EnvironmentSlug}' not found."));
        }

        // Find service with all related data
        var service = await db.Services
            .AsNoTracking()
            .Include(s => s.ServiceNetworks)
                .ThenInclude(sn => sn.Network)
            .Include(s => s.ServiceConfigs)
                .ThenInclude(sc => sc.Config)
            .Include(s => s.ServiceSecrets)
                .ThenInclude(ss => ss.Secret)
            .FirstOrDefaultAsync(s => s.Id == query.ServiceId && s.EnvironmentId == environment.Id, cancellationToken);

        if (service is null)
        {
            return Result.Failure<ServiceDetailDto>(
                Error.NotFound("Service.NotFound", "Service not found."));
        }

        return Result.Success(service.ToDetailDto());
    }
}
