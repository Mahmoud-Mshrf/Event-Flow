using EventFlow.Domain.Common.Results;
using FluentValidation;
using MediatR;

namespace EventFlow.Application.Features.Events.Commands.CancelEvent;

public sealed record CancelEventCommand(
    Guid EventId) : IRequest<Result<Success>>;

public sealed class CancelEventCommandValidator
    : AbstractValidator<CancelEventCommand>
{
    public CancelEventCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("Event id is required.");
    }
}
