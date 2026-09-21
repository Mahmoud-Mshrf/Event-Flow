using EventFlow.Application.Features.Authentication.Commands.Login;
using EventFlow.Domain.Common.Errors;
using EventFlow.Domain.Common.Results;
using MediatR;

namespace EventFlow.Application.Features.Authentication.Commands.RefreshTokenCommand;

public sealed record RefreshTokenCommand(
    string RefreshToken) : IRequest<Result<LoginResponse>>;

public static class RefreshTokenErrors
{
    public static readonly Error InvalidRefreshToken = Error.Failure("RefreshToken.Invalid", "The provided refresh token is invalid or expired.");
    public static readonly Error NotFoundOrInactive = Error.Failure("RefreshToken.NotFoundOrInactive", "The provided refresh token was not found or is inactive.");
}