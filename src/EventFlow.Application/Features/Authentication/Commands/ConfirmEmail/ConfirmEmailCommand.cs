using EventFlow.Domain.Common.Results;
using FluentValidation;
using MediatR;

namespace EventFlow.Application.Features.Authentication.Commands.ConfirmEmail;  

public sealed record ConfirmEmailCommand(
    string Email,
    string Code) : IRequest<Result<Success>>;

public sealed class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Confirmation code is required.")
            .Length(6).WithMessage("Confirmation code must be 6 digits.")
            .Matches(@"^\d{6}$").WithMessage("Confirmation code must contain digits only.");
    }
}

