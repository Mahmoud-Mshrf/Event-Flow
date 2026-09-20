using EventFlow.Application.Common.Helpers;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Identity;
using EventFlow.Domain.Tenants;
using EventFlow.Domain.Users;
using EventFlow.Domain.Users.Enums;
using EventFlow.Domain.Users.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Organizer.Commands.RegisterOrganizer;

public sealed class RegisterOrganizerCommandHandler(
    IAppDbContext db,
    IPasswordHasher passwordHasher) : IRequestHandler<RegisterOrganizerCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(
        RegisterOrganizerCommand request, CancellationToken ct)
    {
        // 1. Check email uniqueness — same rule as attendee registration
        var emailExists = await db.Users
            .AnyAsync(u => u.Email == request.Email, ct);

        if (emailExists)
            return UserErrors.EmailAlreadyInUse;

        // 2. Check organization name uniqueness
        //    Two tenants with the same name causes confusion in public listings
        var orgNameExists = await db.Tenants
            .AnyAsync(t => t.Name == request.OrganizationName, ct);

        if (orgNameExists)
            return TenantErrors.NameAlreadyInUse;

        // 3. Create the Tenant first — User needs TenantId at construction time
        var tenantId = Guid.NewGuid();

        var tenantResult = Tenant.Create(
            tenantId,
            request.OrganizationName,
            request.OrganizationDescription);

        if (tenantResult.IsError)
            return tenantResult.TopError;

        var tenant = tenantResult.Value;

        // 4. Hash password before touching domain
        var passwordHash = passwordHasher.Hash(request.Password);

        // 5. Create the User as Owner of that tenant
        var userResult = User.CreateStaff(
            Guid.NewGuid(),
            request.PhoneNumber,
            request.Name,
            request.Email,
            passwordHash,
            UserRole.Owner,
            tenantId);

        if (userResult.IsError)
            return userResult.TopError;

        var user = userResult.Value;

        // 6. Generate OTP and hash it
        var rawCode = OtpGenerator.Generate();
        var codeHash = passwordHasher.Hash(rawCode);

        var verificationToken = VerificationToken.Create(
            Guid.NewGuid(),
            user.Id,
            VerificationTokenType.EmailConfirmation,
            codeHash,
            validFor: TimeSpan.FromHours(24));

        // 7. Raise domain event — email sends after commit, non-blocking
        user.AddDomainEvent(new OrganizerRegisteredDomainEvent(user.Email, rawCode));

        // 8. Persist everything atomically
        //    Tenant and User are created together or not at all
        await db.Tenants.AddAsync(tenant, ct);
        await db.Users.AddAsync(user, ct);
        await db.VerificationTokens.AddAsync(verificationToken, ct);

        await db.SaveChangesAsync(ct);

        return Result.Success;
    }
}
