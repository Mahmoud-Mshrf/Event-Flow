using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Tickets;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.CheckIn.Commands.CheckInAttendee;

public sealed class CheckInAttendeeCommandHandler(
    IAppDbContext db,
    ICurrentTenant currentTenant,
    IQrCodeService qrCodeService,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<CheckInAttendeeCommand, Result<CheckInResultDto>>
{
    public async Task<Result<CheckInResultDto>> Handle(
        CheckInAttendeeCommand request,
        CancellationToken ct)
    {
        // 1. Tenant guard — check-in staff must belong to a tenant
        if (currentTenant.TenantGuid is not { } tenantId)
            return CheckInErrors.Unauthorized;

        // ── QR VERIFICATION ───────────────────────────────────
        // 2. Verify the QR payload signature and extract the ticketId
        //    If the payload was tampered with or forged, this returns null
        //    This is the security gate — nothing proceeds on invalid QR
        var ticketId = qrCodeService.VerifyAndExtract(request.QrCode);

        if (ticketId is null)
            return TicketErrors.InvalidQrCode;

        // ── TICKET LOOKUP ─────────────────────────────────────
        // 3. Load the ticket with its related data
        //    The global query filter scopes this to the current tenant
        //    automatically — staff at Tenant A cannot see Tenant B's tickets
        var ticket = await db.Tickets
            .Include(t => t.Attendee)
            .Include(t => t.TicketType)
            .FirstOrDefaultAsync(t => t.Id == ticketId.Value, ct);

        // 4. If not found — could be:
        //    a) Ticket genuinely doesn't exist
        //    b) Ticket belongs to a different tenant (filter excluded it)
        //    c) QR was valid signature but ticket was cancelled/deleted
        //    In all cases: return NotFound — never reveal which case it is
        if (ticket is null)
            return TicketErrors.NotFound;

        // ── EVENT BOUNDARY CHECK ──────────────────────────────
        // 5. Confirm this ticket belongs to the event being checked in
        //    Prevents staff at the wrong venue checking in wrong-event tickets
        //    even if they're in the same tenant
        //    Example: "Cairo .NET Conference" staff scanning tickets for
        //    "SQL Server Workshop" — same tenant, different event, should fail
        if (ticket.EventId != request.EventId)
            return TicketErrors.WrongEvent;

        // ── DOMAIN CHECK-IN ───────────────────────────────────
        // 6. Call the domain method — enforces all business rules:
        //    - Already checked in? → TicketErrors.AlreadyCheckedIn
        //    - Cancelled? → TicketErrors.TicketCancelled
        //    - Not Valid status? → TicketErrors.CannotCheckIn
        //    The domain method also raises AttendeeCheckedInDomainEvent
        //    which triggers the SignalR real-time dashboard update
        var checkInResult = ticket.CheckIn(dateTimeProvider.UtcNow);

        if (checkInResult.IsError)
            return checkInResult.TopError;

        // 7. Persist — SaveChangesAsync dispatches AttendeeCheckedInDomainEvent
        //    The SignalR push happens after this commit succeeds
        await db.SaveChangesAsync(ct);

        // 8. Return enough info for the check-in UI to show a confirmation screen
        //    "✓ Ahmed Mohamed — VIP — Checked in at 14:32"
        return new CheckInResultDto(
            ticket.TicketNumber,
            ticket.Attendee.Name,
            ticket.TicketType.Name,
            ticket.CheckedInAt!.Value);
    }
}
