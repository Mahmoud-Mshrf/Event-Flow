using EventFlow.Domain.Events;
using FluentValidation;

namespace EventFlow.Application.Features.Events.CreateEvent;

public sealed class CreateEventCommandValidator
    : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.EventName)
            .NotEmpty()
            .WithMessage(EventErrors.InvalidName.Description)
            .Length(6, 100)
            .WithMessage(EventErrors.InvalidName.Description);

        RuleFor(x => x.Description)
            .Must(description =>
                string.IsNullOrWhiteSpace(description) ||
                (description.Length >= 6 && description.Length <= 500))
            .WithMessage(EventErrors.InvalidDescription.Description);

        RuleFor(x => x.Location)
            .NotEmpty()
            .WithMessage(EventErrors.InvalidLocation.Description);

        RuleFor(x => x.StartDate)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage(EventErrors.InvalidSchedule.Description);

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage(EventErrors.InvalidSchedule.Description);

        RuleFor(x => x.RegistrationStart)
            .LessThan(x => x.RegistrationEnd)
            .WithMessage(EventErrors.InvalidRegistrationPeriod.Description);

        RuleFor(x => x.RegistrationStart)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage(EventErrors.InvalidRegistrationPeriod.Description);

        RuleFor(x => x.RegistrationEnd)
            .LessThanOrEqualTo(x => x.StartDate)
            .WithMessage(EventErrors.InvalidRegistrationPeriod.Description);

        RuleFor(x => x.Visibility)
            .IsInEnum()
            .WithMessage(EventErrors.InvalidVisibility.Description);
    }
}
