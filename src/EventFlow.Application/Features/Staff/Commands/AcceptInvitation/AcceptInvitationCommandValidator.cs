using FluentValidation;

namespace EventFlow.Application.Features.Staff.Commands.AcceptInvitation;

public sealed class AcceptInvitationCommandValidator
    : AbstractValidator<AcceptInvitationCommand>
{
    public AcceptInvitationCommandValidator()
    {
        RuleFor(x => x.InviteeEmail)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Token)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100);
    }
}