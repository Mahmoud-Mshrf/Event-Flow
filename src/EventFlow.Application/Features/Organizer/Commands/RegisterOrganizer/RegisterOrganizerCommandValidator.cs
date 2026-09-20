using FluentValidation;

namespace EventFlow.Application.Features.Organizer.Commands.RegisterOrganizer;

public sealed class RegisterOrganizerCommandValidator 
    : AbstractValidator<RegisterOrganizerCommand>
{
    public RegisterOrganizerCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MinimumLength(6).WithMessage("Name must be at least 6 characters.")
            .MaximumLength(100).WithMessage("Name must be at most 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+?\d{7,15}$")
            .WithMessage("Phone number must be 7–15 digits and may start with '+'.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"\d").WithMessage("Password must contain at least one digit.")
            .Matches(@"[@$!%*?&]")
            .WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.OrganizationName)
            .NotEmpty().WithMessage("Organization name is required.")
            .MinimumLength(6).WithMessage("Organization name must be at least 6 characters.")
            .MaximumLength(100).WithMessage("Organization name must be at most 100 characters.");

        RuleFor(x => x.OrganizationDescription)
            .MaximumLength(500)
            .WithMessage("Organization description must be at most 500 characters.")
            .When(x => x.OrganizationDescription is not null);
    }
}