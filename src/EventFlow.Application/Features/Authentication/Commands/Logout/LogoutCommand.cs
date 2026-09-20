using EventFlow.Domain.Common.Results;
using MediatR;

namespace EventFlow.Application.Features.Authentication.Commands.Logout;

public sealed record LogoutCommand(
    string RefreshToken) : IRequest<Result<Success>>;
