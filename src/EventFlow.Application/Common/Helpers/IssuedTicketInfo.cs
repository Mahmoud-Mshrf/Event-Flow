namespace EventFlow.Application.Common.Helpers;

public sealed record IssuedTicketInfo(
    string TicketNumber,
    string TicketTypeName,
    string QrCode);
