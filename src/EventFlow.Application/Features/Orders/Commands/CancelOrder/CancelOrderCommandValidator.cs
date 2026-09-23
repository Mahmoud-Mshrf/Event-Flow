using FluentValidation;

namespace EventFlow.Application.Features.Orders.Commands.CancelOrder;

public sealed class CancelOrderCommandValidator
    : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order id is required.");
    }
}
