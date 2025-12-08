namespace BaseDock.Application.Abstractions.Data;

using BaseDock.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }

    DbSet<Session> Sessions { get; }

    DbSet<Project> Projects { get; }

    DbSet<ProjectMember> ProjectMembers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
