using EventFlow.Domain.Common.Results;
using MediatR;

namespace EventFlow.Application.Features.Organizer.Commands.RegisterOrganizer;

public sealed record RegisterOrganizerCommand(
    string Name,
    string Email,
    string PhoneNumber,
    string Password,
    string OrganizationName,
    string? OrganizationDescription) : IRequest<Result<Success>>;
