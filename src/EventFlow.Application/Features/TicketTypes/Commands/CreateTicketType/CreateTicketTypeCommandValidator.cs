using EventFlow.Domain.TicketTypes;
using FluentValidation;

namespace EventFlow.Application.Features.TicketTypes.commands.CreateTicketType;

public sealed class CreateTicketTypeCommandValidator
    : AbstractValidator<CreateTicketTypeCommand>
{
    public CreateTicketTypeCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("Event id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(TicketTypeErrors.InvalidName.Description)
            .Length(3, 30).WithMessage(TicketTypeErrors.InvalidName.Description);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage(TicketTypeErrors.InvalidPrice.Description);

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage(TicketTypeErrors.InvalidCapacity.Description);

        RuleFor(x => x.SalesStart)
            .LessThan(x => x.SalesEnd)
            .WithMessage(TicketTypeErrors.InvalidSalesWindow.Description);
    }
}