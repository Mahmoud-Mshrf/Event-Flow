using EventFlow.Domain.TicketTypes;
using FluentValidation;

namespace EventFlow.Application.Features.TicketTypes.Commands.UpdateTicketTypeSalesWindow;

public sealed class UpdateTicketTypeSalesWindowCommandValidator
    : AbstractValidator<UpdateTicketTypeSalesWindowCommand>
{
    public UpdateTicketTypeSalesWindowCommandValidator()
    {
        RuleFor(x => x.TicketTypeId)
            .NotEmpty().WithMessage("Ticket type id is required.");

        RuleFor(x => x.SalesStart)
            .LessThan(x => x.SalesEnd)
            .WithMessage(TicketTypeErrors.InvalidSalesWindow.Description);
    }
}
