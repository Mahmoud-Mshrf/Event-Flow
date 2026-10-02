namespace EventFlow.Application.Features.CheckIn.Dtos;

public sealed record EventDashboardUpdate(
    Guid EventId,
    int TotalTickets,
    int CheckedIn,
    int Remaining,
    DateTime LastCheckInAt);