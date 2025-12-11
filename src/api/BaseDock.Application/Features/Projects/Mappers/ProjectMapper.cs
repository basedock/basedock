namespace BaseDock.Application.Features.Projects.Mappers;

using BaseDock.Application.Features.Projects.DTOs;
using BaseDock.Domain.Entities;

public static class ProjectMapper
{
    public static ProjectDto ToDto(this Project entity)
    {
        return new ProjectDto(
            entity.Id,
            entity.Name,
            entity.Slug,
            entity.Description,
            entity.CreatedByUserId,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.Environments.Count,
            entity.Members.ToDtos());
    }

    public static IEnumerable<ProjectDto> ToDtos(this IEnumerable<Project> entities)
    {
        return entities.Select(e => e.ToDto());
    }

    public static ProjectMemberDto ToDto(this ProjectMember member)
    {
        return new ProjectMemberDto(
            member.UserId,
            member.User.Email,
            member.User.DisplayName,
            member.JoinedAt);
    }

    public static IEnumerable<ProjectMemberDto> ToDtos(this IEnumerable<ProjectMember> members)
    {
        return members.Select(m => m.ToDto());
    }
}
