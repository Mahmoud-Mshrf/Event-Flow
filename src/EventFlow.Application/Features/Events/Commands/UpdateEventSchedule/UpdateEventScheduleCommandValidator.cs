using EventFlow.Domain.Events;
using FluentValidation;

namespace EventFlow.Application.Features.Events.Commands.UpdateEventSchedule;

public sealed class UpdateEventScheduleCommandValidator
    : AbstractValidator<UpdateEventScheduleCommand>
{
    public UpdateEventScheduleCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("Event id is required.");

        RuleFor(x => x.StartDate)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage(EventErrors.InvalidSchedule.Description);

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage(EventErrors.InvalidSchedule.Description);
    }
}