using System.Text.RegularExpressions;
using EventFlow.Domain.Common;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Events;
using EventFlow.Domain.Orders;
using EventFlow.Domain.Tenants;
using EventFlow.Domain.Tickets;
using EventFlow.Domain.Users.Enums;

namespace EventFlow.Domain.Users;

public class User : AuditableEntity
{
    public string PhoneNumber { get; private set; } = null!;
    public Guid? TenantId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public UserRole? Role { get; private set; }
    public bool Disabled { get; private set; }
    public bool EmailConfirmed { get; private set; }

    public Tenant? Tenant { get; private set; }

    private User(Guid id, string phoneNumber, string name, string email, string passwordHash, UserRole? role, Guid? tenantId)
        : base(id)
    {
        PhoneNumber = phoneNumber;
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        TenantId = tenantId;
        Disabled = false;
    }

    public static Result<User> CreateStaff(
        Guid id, string phoneNumber, string name, string email, string passwordHash,
        UserRole role, Guid tenantId)
    {
        var validation = ValidateCommonFields(phoneNumber, name, email, passwordHash);
        if (validation.IsError)
            return validation.TopError;

        if (tenantId == Guid.Empty)
            return UserErrors.TenantIdRequired;

        if (!Enum.IsDefined(typeof(UserRole), role))
            return UserErrors.InvalidRole;

        return new User(id, phoneNumber, name, email, passwordHash, role, tenantId);
    }

    public static Result<User> CreateAttendee(
        Guid id, string phoneNumber, string name, string email, string passwordHash)
    {
        var validation = ValidateCommonFields(phoneNumber, name, email, passwordHash);
        if (validation.IsError)
            return validation.TopError;

        return new User(id, phoneNumber, name, email, passwordHash, role: null, tenantId: null);
    }

    private static Result<Success> ValidateCommonFields(
        string phoneNumber, string name, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber) || !Regex.IsMatch(phoneNumber, @"^\+?\d{7,15}$"))
            return UserErrors.InvalidPhoneNumber;

        if (string.IsNullOrWhiteSpace(name))
            return UserErrors.NameIsRequired;

        if (name.Length < 6 || name.Length > 100)
            return UserErrors.InvalidName;

        if (string.IsNullOrWhiteSpace(email))
            return UserErrors.EmailIsRequired;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            if (addr.Address != email)
                return UserErrors.InvalidEmail;
        }
        catch
        {
            return UserErrors.InvalidEmail;
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
            return UserErrors.PasswordIsRequired;

        return Result.Success;
    }
    public Result<Success> ConfirmEmail()
    {
        if (EmailConfirmed)
            return UserErrors.EmailAlreadyConfirmed;

        EmailConfirmed = true;
        return Result.Success;
    }
    public Result<Updated> UpdatePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber) || !Regex.IsMatch(phoneNumber, @"^\+?\d{7,15}$"))
            return UserErrors.InvalidPhoneNumber;

        PhoneNumber = phoneNumber;
        return Result.Updated;
    }

    public Result<Updated> UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 6 || name.Length > 100)
            return UserErrors.InvalidName;

        Name = name;
        return Result.Updated;
    }

    public Result<Updated> ChangeRole(UserRole role)
    {
        if (TenantId is null)
            return UserErrors.CannotAssignRoleToAttendee;

        if (!Enum.IsDefined(typeof(UserRole), role))
            return UserErrors.InvalidRole;

        Role = role;
        return Result.Updated;
    }

    public Result<Success> ResetPassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            return UserErrors.PasswordIsRequired;

        PasswordHash = newPasswordHash;
        return Result.Success;
    }

    public Result<Updated> Disable()
    {
        Disabled = true;
        return Result.Updated;
    }

    public Result<Updated> Enable()
    {
        Disabled = false;
        return Result.Updated;
    }
}