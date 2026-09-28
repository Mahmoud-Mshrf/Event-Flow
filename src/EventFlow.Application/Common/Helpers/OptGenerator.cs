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

public static class TicketNumberGenerator
{
    // Format: EVF-{yyyyMMdd}-{random 6 digits}
    // Example: EVF-20261001-482910
    // Readable, sortable by date, unique enough for MVP
    public static string Generate()
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = RandomNumberGenerator.GetInt32(100000, 999999);
        return $"EVF-{date}-{random}";
    }
}

