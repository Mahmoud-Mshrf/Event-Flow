using EventFlow.Domain.Common.Errors;

namespace EventFlow.Application.Features.Authentication.Commands.Logout;

public static class LogoutErrors
{
    public static readonly Error Unauthenticated =
        Error.Unauthorized("Logout.Unauthenticated", "You must be logged in to log out.");
}