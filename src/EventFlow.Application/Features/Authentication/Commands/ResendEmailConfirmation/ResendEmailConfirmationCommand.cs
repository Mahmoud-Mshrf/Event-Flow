using EventFlow.Domain.Common.Results;
using MediatR;

namespace EventFlow.Application.Features.Authentication.Commands.ResendEmailConfirmation;

public sealed record ResendEmailConfirmationCommand(
    string Email) : IRequest<Result<Success>>;
