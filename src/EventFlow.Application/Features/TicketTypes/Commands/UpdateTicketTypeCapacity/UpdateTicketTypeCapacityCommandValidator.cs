using EventFlow.Domain.TicketTypes;
using FluentValidation;

namespace EventFlow.Application.Features.TicketTypes.Commands.UpdateTicketTypeCapacity;

public sealed class UpdateTicketTypeCapacityCommandValidator
    : AbstractValidator<UpdateTicketTypeCapacityCommand>
{
    public UpdateTicketTypeCapacityCommandValidator()
    {
        RuleFor(x => x.TicketTypeId)
            .NotEmpty().WithMessage("Ticket type id is required.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0)
            .WithMessage(TicketTypeErrors.InvalidCapacity.Description);
    }
}
