namespace BaseDock.Application.Features.Users.DTOs;

public sealed record UserDto(
    Guid Id,
    string Email,
    string DisplayName,
    bool IsAdmin,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record CreateUserRequest(
    string Email,
    string DisplayName,
    string Password);

public sealed record UpdateUserRequest(
    string DisplayName,
    string? Email);
