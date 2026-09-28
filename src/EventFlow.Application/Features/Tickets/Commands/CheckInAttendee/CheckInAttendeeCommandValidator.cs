using FluentValidation;

namespace EventFlow.Application.Features.Tickets.Commands.CheckInAttendee;

public sealed class CheckInAttendeeCommandValidator
    : AbstractValidator<CheckInAttendeeCommand>
{
    public CheckInAttendeeCommandValidator()
    {
        RuleFor(x => x.QrCode)
            .NotEmpty().WithMessage("QR code is required.");

        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("Event id is required.");
    }
}
