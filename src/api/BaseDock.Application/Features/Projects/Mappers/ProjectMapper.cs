namespace BaseDock.Application.Features.Projects.Mappers;

using BaseDock.Application.Features.Projects.DTOs;
using BaseDock.Domain.Entities;
using Riok.Mapperly.Abstractions;

[Mapper]
public static partial class ProjectMapper
{
    [MapperIgnoreSource(nameof(Project.CreatedBy))]
    public static partial ProjectDto ToDto(this Project entity);

    public static partial IEnumerable<ProjectDto> ToDtos(this IEnumerable<Project> entities);

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
