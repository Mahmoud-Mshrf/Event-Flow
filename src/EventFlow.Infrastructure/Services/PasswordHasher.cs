using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace EventFlow.Infrastructure.Services;

public sealed class PasswordHasher : IPasswordHasher
{
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<User> _hasher = new();

    public string Hash(string password) =>
        _hasher.HashPassword(user: null!, password); // user param unused by the algorithm itself

    public bool Verify(string password, string hash)
    {
        var result = _hasher.VerifyHashedPassword(user: null!, hash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}