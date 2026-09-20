namespace EventFlow.Application.Common.Interfaces;

public interface ITokenSettings
{
    string Issuer { get; }

    string Audience { get; }

    string SigningKey { get; }

    int AccessTokenMinutes { get; }

    int RefreshTokenDays { get; }
}