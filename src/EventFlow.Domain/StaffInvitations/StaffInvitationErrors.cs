using EventFlow.Domain.Common.Errors;

namespace EventFlow.Domain.StaffInvitations;

public static class StaffInvitationErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "StaffInvitation.NotFound",
        "Invitation not found.");

    public static readonly Error AlreadyAccepted = Error.Conflict(
        "StaffInvitation.AlreadyAccepted",
        "This invitation has already been accepted.");

    public static readonly Error Revoked = Error.Conflict(
        "StaffInvitation.Revoked",
        "This invitation has been revoked.");

    public static readonly Error Expired = Error.Conflict(
        "StaffInvitation.Expired",
        "This invitation has expired.");

    public static readonly Error PendingInvitationExists = Error.Conflict(
        "StaffInvitation.PendingInvitationExists",
        "A pending invitation already exists for this email address.");

    public static readonly Error CannotRevoke = Error.Conflict(
        "StaffInvitation.CannotRevoke",
        "Only pending invitations can be revoked.");

    public static readonly Error InvalidToken = Error.Validation(
        "StaffInvitation.InvalidToken",
        "The invitation token is invalid.");
}
