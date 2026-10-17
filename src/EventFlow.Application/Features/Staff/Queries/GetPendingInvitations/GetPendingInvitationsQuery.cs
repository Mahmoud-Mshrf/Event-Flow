using EventFlow.Application.Common.Contracts;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Staff.Dtos;
using EventFlow.Domain.Common.Results;

namespace EventFlow.Application.Features.Staff.Queries.GetPendingInvitations;

public sealed record GetPendingInvitationsQuery : PageRequest,
    ICachedQuery<Result<PaginatedList<PendingInvitationDto>>>
{
    public Guid? TenantId { get; init; }

    public string CacheKey =>
        $"tenant-invitations:{TenantId}:{Page}:{PageSize}";

    public string[] Tags =>
    [
        $"tenant-{TenantId}-invitations"
    ];

    // Shorter TTL than staff: "pending" depends on the clock, not only on
    // commands, so an invitation can expire while a cached page is still alive.
    public TimeSpan Expiration => TimeSpan.FromMinutes(2);
}
