using EventFlow.Domain.Common.Results;
using MediatR;

namespace EventFlow.Application.Features.Authentication.Commands.ResetPassword;

public sealed record ResetPasswordCommand(
    string Email,
    string Code,
    string NewPassword,
    string ConfirmPassword) : IRequest<Result<Success>>;
