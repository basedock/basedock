namespace BaseDock.Application.Features.Users.Mappers;

using BaseDock.Application.Features.Users.DTOs;
using BaseDock.Domain.Entities;
using Riok.Mapperly.Abstractions;

[Mapper]
public static partial class UserMapper
{
    [MapperIgnoreSource(nameof(User.PasswordHash))]
    public static partial UserDto ToDto(this User entity);

    public static partial IEnumerable<UserDto> ToDtos(this IEnumerable<User> entities);
}
