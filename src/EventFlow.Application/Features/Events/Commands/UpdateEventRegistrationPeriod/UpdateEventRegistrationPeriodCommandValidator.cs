using EventFlow.Domain.Events;
using FluentValidation;

namespace EventFlow.Application.Features.Events.Commands.UpdateEventRegistrationPeriod;

public sealed class UpdateEventRegistrationPeriodCommandValidator
    : AbstractValidator<UpdateEventRegistrationPeriodCommand>
{
    public UpdateEventRegistrationPeriodCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("Event id is required.");

        RuleFor(x => x.RegistrationStart)
            .LessThan(x => x.RegistrationEnd)
            .WithMessage(EventErrors.InvalidRegistrationPeriod.Description);
    }
}