using EventFlow.Application.Features.Authentication.Commands.ConfirmEmail;
using EventFlow.Application.Features.Authentication.Commands.Login;
using EventFlow.Application.Features.Authentication.Commands.Logout;
using EventFlow.Application.Features.Authentication.Commands.RefreshTokenCommand;
using EventFlow.Application.Features.Authentication.Commands.RegisterAttendee;
using EventFlow.Application.Features.Authentication.Commands.RequestPasswordReset;
using EventFlow.Application.Features.Authentication.Commands.ResendEmailConfirmation;
using EventFlow.Application.Features.Authentication.Commands.ResetPassword;
using EventFlow.Application.Features.Organizer.Commands.RegisterOrganizer;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.Api.Controllers;

[Route("api/auth")]
public sealed class AuthController(ISender sender) : ApiController
{
    [HttpPost("register/attendee")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Register a new attendee account.")]
    [EndpointName("RegisterAttendee")]
    public async Task<ActionResult> RegisterAttendee(
        [FromBody] RegisterAttendeeCommand command,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }

    [HttpPost("register/organizer")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Register a new organizer account and organization.")]
    [EndpointName("RegisterOrganizer")]
    public async Task<ActionResult> RegisterOrganizer(
        [FromBody] RegisterOrganizerCommand command,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }

    [HttpPost("confirm-email")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [EndpointSummary("Confirm email address using the OTP sent at registration.")]
    [EndpointName("ConfirmEmail")]
    public async Task<ActionResult> ConfirmEmail(
        [FromBody] ConfirmEmailCommand command,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }

    [HttpPost("resend-confirmation")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [EndpointSummary("Resend the email confirmation OTP.")]
    [EndpointName("ResendEmailConfirmation")]
    public async Task<ActionResult> ResendEmailConfirmation(
        [FromBody] ResendEmailConfirmationCommand command,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [EndpointSummary("Login and receive an access token and refresh token.")]
    [EndpointName("Login")]
    public async Task<ActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [EndpointSummary("Rotate a refresh token and receive a new access token.")]
    [EndpointName("RefreshToken")]
    public async Task<ActionResult> RefreshToken(
        [FromBody] RefreshTokenCommand command,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [EndpointSummary("Revoke the current session's refresh token.")]
    [EndpointName("Logout")]
    public async Task<ActionResult> Logout(
        [FromBody] LogoutCommand command,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [EndpointSummary("Request a password reset OTP.")]
    [EndpointName("RequestPasswordReset")]
    public async Task<ActionResult> RequestPasswordReset(
        [FromBody] RequestPasswordResetCommand command,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [EndpointSummary("Reset password and revoke all active sessions.")]
    [EndpointName("ResetPassword")]
    public async Task<ActionResult> ResetPassword(
        [FromBody] ResetPasswordCommand command,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }
}