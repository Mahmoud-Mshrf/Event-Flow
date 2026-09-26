using FluentValidation;

namespace EventFlow.Application.Features.Payments.Commands.InitiatePayment;

public sealed class InitiatePaymentCommandValidator
    : AbstractValidator<InitiatePaymentCommand>
{
    public InitiatePaymentCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order id is required.");
    }
}

// No validator for HandleWebhookCommand
// Validation is the gateway's job (signature verification)
// FluentValidation doesn't belong in webhook security