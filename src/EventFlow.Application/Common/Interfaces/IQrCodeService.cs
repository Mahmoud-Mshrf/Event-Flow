namespace EventFlow.Application.Common.Interfaces;

public interface IQrCodeService
{
    // Generates a signed QR payload for a ticket
    // Format: {ticketId}.{hmac-signature}
    string GeneratePayload(Guid ticketId);

    // Verifies the payload and extracts the ticketId
    // Returns null if the payload is invalid or tampered
    Guid? VerifyAndExtract(string qrPayload);
}