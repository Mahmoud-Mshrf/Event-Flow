using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.CheckIn.Commands.CheckInAttendee;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Tickets.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.CheckIn.Queries.GetCheckInStats;

public sealed class GetCheckInStatsQueryHandler(
    IAppDbContext db,
    ICurrentTenant currentTenant)
    : IRequestHandler<GetCheckInStatsQuery, Result<CheckInStatsDto>>
{
    public async Task<Result<CheckInStatsDto>> Handle(
        GetCheckInStatsQuery request,
        CancellationToken ct)
    {
        if (currentTenant.TenantGuid is not { } tenantId)
            return CheckInErrors.Unauthorized;

        // Group by status in one query — more efficient than two separate counts
        var stats = await db.Tickets
            .AsNoTracking()
            .Where(t => t.EventId == request.EventId)
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var total = stats.Sum(s => s.Count);
        var checkedIn = stats
            .FirstOrDefault(s => s.Status == TicketStatus.CheckedIn)?.Count ?? 0;

        return new CheckInStatsDto(
            request.EventId,
            total,
            checkedIn,
            total - checkedIn);
    }
}