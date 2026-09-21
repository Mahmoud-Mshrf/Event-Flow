using EventFlow.Domain.Events;
using FluentValidation;

namespace EventFlow.Application.Features.Events.Commands.UpdateEventDetails;

public sealed class UpdateEventDetailsCommandValidator
    : AbstractValidator<UpdateEventDetailsCommand>
{
    public UpdateEventDetailsCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("Event id is required.");

        RuleFor(x => x.EventName)
            .NotEmpty()
            .WithMessage(EventErrors.InvalidName.Description)
            .Length(6, 100)
            .WithMessage(EventErrors.InvalidName.Description);

        RuleFor(x => x.Description)
            .Must(d => string.IsNullOrWhiteSpace(d) || (d.Length >= 6 && d.Length <= 500))
            .WithMessage(EventErrors.InvalidDescription.Description)
            .When(x => x.Description is not null);
    }
}
