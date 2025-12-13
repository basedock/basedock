namespace BaseDock.Application.Features.Secrets.Queries.GetSecrets;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Secrets.DTOs;
using BaseDock.Application.Features.Secrets.Mappers;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class GetSecretsQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetSecretsQuery, Result<IEnumerable<SecretDto>>>
{
    public async Task<Result<IEnumerable<SecretDto>>> HandleAsync(
        GetSecretsQuery query,
        CancellationToken cancellationToken = default)
    {
        var project = await db.Projects
            .AsNoTracking()
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Slug == query.ProjectSlug, cancellationToken);

        if (project is null)
        {
            return Result.Failure<IEnumerable<SecretDto>>(
                Error.NotFound("Project.NotFound", $"Project with slug '{query.ProjectSlug}' not found."));
        }

        if (!project.Members.Any(m => m.UserId == query.UserId))
        {
            return Result.Failure<IEnumerable<SecretDto>>(
                Error.Forbidden("You are not a member of this project."));
        }

        var environment = await db.Environments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ProjectId == project.Id && e.Slug == query.EnvironmentSlug, cancellationToken);

        if (environment is null)
        {
            return Result.Failure<IEnumerable<SecretDto>>(
                Error.NotFound("Environment.NotFound", $"Environment with slug '{query.EnvironmentSlug}' not found."));
        }

        var secrets = await db.Secrets
            .AsNoTracking()
            .Where(s => s.EnvironmentId == environment.Id)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        return Result.Success(secrets.ToDtos());
    }
}
