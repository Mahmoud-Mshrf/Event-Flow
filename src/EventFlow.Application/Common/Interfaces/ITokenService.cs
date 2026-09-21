using EventFlow.Domain.Users;

namespace EventFlow.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken(); // raw, random — caller hashes it before storing
    Guid? GetUserIdFromExpiredToken(string accessToken);
}
