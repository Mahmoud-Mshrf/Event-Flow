using FluentValidation;

namespace EventFlow.Application.Features.Authentication.Commands.RequestPasswordReset;

public sealed class RequestPasswordResetCommandValidator 
    : AbstractValidator<RequestPasswordResetCommand>
{
    public RequestPasswordResetCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");
    }
}