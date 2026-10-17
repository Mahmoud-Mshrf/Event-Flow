using FluentValidation;

namespace EventFlow.Application.Features.Staff.Commands.InviteStaff;

public sealed class InviteStaffCommandValidator
    : AbstractValidator<InviteStaffCommand>
{
    public InviteStaffCommandValidator()
    {
        RuleFor(x => x.InviteeEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Role)
            .IsInEnum()
            .Must(r => r == Domain.Users.Enums.UserRole.Employee)
            .WithMessage("Only Employee role can be invited. " +
                         "Owners are created via organizer registration.");
    }
}
