using EventFlow.Domain.Events;
using FluentValidation;

namespace EventFlow.Application.Features.Events.Commands.UpdateEventVisibility;

public sealed class UpdateEventVisibilityCommandValidator
    : AbstractValidator<UpdateEventVisibilityCommand>
{
    public UpdateEventVisibilityCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("Event id is required.");

        RuleFor(x => x.Visibility)
            .IsInEnum()
            .WithMessage(EventErrors.InvalidVisibility.Description);
    }
}