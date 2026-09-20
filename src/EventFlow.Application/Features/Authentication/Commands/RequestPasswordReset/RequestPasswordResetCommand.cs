using EventFlow.Domain.Common.Results;
using MediatR;

namespace EventFlow.Application.Features.Authentication.Commands.RequestPasswordReset;

public sealed record RequestPasswordResetCommand(
    string Email) : IRequest<Result<Success>>;
