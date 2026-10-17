using EventFlow.Domain.Users.Enums;

namespace EventFlow.Application.Features.Staff.Dtos;

public sealed record StaffMemberDto(
    Guid Id,
    string Name,
    string Email,
    UserRole? Role,
    bool Disabled);