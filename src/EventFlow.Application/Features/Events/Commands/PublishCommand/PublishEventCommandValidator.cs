using FluentValidation;

namespace EventFlow.Application.Features.Events.Commands.PublishCommand;

public sealed class PublishEventCommandValidator
    : AbstractValidator<PublishEventCommand>
{
    public PublishEventCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("Event id is required.");
    }
}
