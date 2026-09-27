using System.Security.Cryptography;
using System.Text;
using EventFlow.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace EventFlow.Infrastructure.Services;

// EventFlow.Infrastructure/Services/QrCodeService.cs
public sealed class QrCodeService(IOptions<QrCodeSettings> settings) : IQrCodeService
{
    private readonly QrCodeSettings _settings = settings.Value;

    public string GeneratePayload(Guid ticketId)
    {
        var payload = ticketId.ToString();
        var signature = ComputeSignature(payload);
        // Format: "ticketId.signature"
        // The dot separator makes splitting reliable
        return $"{payload}.{signature}";
    }

    public Guid? VerifyAndExtract(string qrPayload)
    {
        if (string.IsNullOrWhiteSpace(qrPayload))
            return null;

        // Split on the LAST dot — ticketId is a GUID which contains no dots
        // signature is base64 which also contains no dots
        // So splitting on '.' gives exactly two parts
        var dotIndex = qrPayload.LastIndexOf('.');
        if (dotIndex < 0) return null;

        var ticketIdStr = qrPayload[..dotIndex];
        var signature = qrPayload[(dotIndex + 1)..];

        // Verify the signature
        var expectedSignature = ComputeSignature(ticketIdStr);
        if (!expectedSignature.Equals(signature, StringComparison.Ordinal))
            return null;

        // Parse the ticketId
        return Guid.TryParse(ticketIdStr, out var ticketId)
            ? ticketId
            : null;
    }

    private string ComputeSignature(string payload)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_settings.Secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(payloadBytes);
        // URL-safe base64 — no +, /, = characters that cause issues in QR URLs
        return Convert.ToBase64String(hash)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}