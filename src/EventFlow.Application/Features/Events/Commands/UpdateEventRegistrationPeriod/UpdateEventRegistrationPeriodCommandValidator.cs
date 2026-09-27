using EventFlow.Domain.Events;
using EventFlow.Application.Common.Interfaces;
using FluentValidation;

namespace EventFlow.Application.Features.Events.Commands.UpdateEventRegistrationPeriod;

public sealed class UpdateEventRegistrationPeriodCommandValidator
    : AbstractValidator<UpdateEventRegistrationPeriodCommand>
{
    public UpdateEventRegistrationPeriodCommandValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("Event id is required.");

        RuleFor(x => x.RegistrationStart)
            .LessThan(x => x.RegistrationEnd)
            .WithMessage(EventErrors.InvalidRegistrationPeriod.Description);

        RuleFor(x => x.RegistrationStart)
            .GreaterThan(dateTimeProvider.UtcNow)
            .WithMessage(EventErrors.InvalidRegistrationPeriod.Description);

        RuleFor(x => x.RegistrationEnd)
            .GreaterThan(dateTimeProvider.UtcNow)
            .WithMessage(EventErrors.InvalidRegistrationPeriod.Description);
    }
}