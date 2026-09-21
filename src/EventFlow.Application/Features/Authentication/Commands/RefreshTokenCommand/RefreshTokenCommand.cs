using EventFlow.Application.Features.Authentication.Commands.Login;
using EventFlow.Domain.Common.Errors;
using EventFlow.Domain.Common.Results;
using MediatR;

namespace EventFlow.Application.Features.Authentication.Commands.RefreshTokenCommand;

public sealed record RefreshTokenCommand(
    string AccessToken,
    string RefreshToken) : IRequest<Result<LoginResponse>>;
public static class RefreshTokenErrors
{
    public static readonly Error InvalidAccessToken =
        Error.Unauthorized("RefreshToken.InvalidAccessToken",
            "The access token is invalid or tampered.");

    public static readonly Error NotFoundOrInactive =
        Error.Unauthorized("RefreshToken.NotFoundOrInactive",
            "The refresh token was not found or has expired.");
}