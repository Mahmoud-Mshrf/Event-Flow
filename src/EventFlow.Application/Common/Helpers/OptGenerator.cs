using System.Security.Cryptography;

namespace EventFlow.Application.Common.Helpers;

public static class OtpGenerator
{
    public static string Generate()
    {
        var randomNumber = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return randomNumber.ToString("D6");
    }
}