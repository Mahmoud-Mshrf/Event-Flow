using EventFlow.Domain.TicketTypes;
using FluentValidation;

namespace EventFlow.Application.Features.TicketTypes.Commands.UpdateTicketTypeName;

public sealed class UpdateTicketTypeNameCommandValidator
    : AbstractValidator<UpdateTicketTypeNameCommand>
{
    public UpdateTicketTypeNameCommandValidator()
    {
        RuleFor(x => x.TicketTypeId)
            .NotEmpty().WithMessage("Ticket type id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(TicketTypeErrors.InvalidName.Description)
            .Length(3, 30).WithMessage(TicketTypeErrors.InvalidName.Description);
    }
}
