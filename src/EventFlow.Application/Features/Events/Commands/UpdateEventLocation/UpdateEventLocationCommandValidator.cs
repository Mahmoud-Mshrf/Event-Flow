using EventFlow.Domain.Events;
using FluentValidation;

namespace EventFlow.Application.Features.Events.Commands.UpdateEventLocation;

public sealed class UpdateEventLocationCommandValidator
    : AbstractValidator<UpdateEventLocationCommand>
{
    public UpdateEventLocationCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("Event id is required.");

        RuleFor(x => x.Location)
            .NotEmpty()
            .WithMessage(EventErrors.InvalidLocation.Description);
    }
}