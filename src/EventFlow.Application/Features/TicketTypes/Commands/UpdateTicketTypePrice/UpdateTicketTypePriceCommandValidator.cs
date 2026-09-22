using EventFlow.Domain.TicketTypes;
using FluentValidation;

namespace EventFlow.Application.Features.TicketTypes.Commands.UpdateTicketTypePrice;

public sealed class UpdateTicketTypePriceCommandValidator
    : AbstractValidator<UpdateTicketTypePriceCommand>
{
    public UpdateTicketTypePriceCommandValidator()
    {
        RuleFor(x => x.TicketTypeId)
            .NotEmpty().WithMessage("Ticket type id is required.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .WithMessage(TicketTypeErrors.InvalidPrice.Description);
    }
}
