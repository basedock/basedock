namespace BaseDock.Application.Abstractions.Security;

using System.Security.Claims;
using BaseDock.Domain.Entities;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    ClaimsPrincipal? ValidateAccessToken(string token);
    string? GetTokenId(string token);
    DateTime GetTokenExpiration(string token);
}
