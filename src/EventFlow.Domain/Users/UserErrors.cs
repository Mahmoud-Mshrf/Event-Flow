using EventFlow.Domain.Common.Errors;

namespace EventFlow.Domain.Users;

public static class UserErrors
{
    public static readonly Error InvalidPhoneNumber = Error.Validation("User.InvalidPhoneNumber", "Phone number must be 7–15 digits and may start with '+'.");
    public static readonly Error InvalidEmail = Error.Validation("User.InvalidEmail", "The email is invalid.");
    public static readonly Error InvalidPassword = Error.Validation("User.InvalidPassword", "The password is invalid, it must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one digit, and one special character.");
    public static readonly Error InvalidRole = Error.Validation("User.InvalidRole", "The role is invalid.");
    public static readonly Error InvalidName = Error.Validation("User.InvalidName", "The name is invalid, it must be between 6 and 100 characters.");
    public static readonly Error EmailIsRequired = Error.Validation("User.EmailIsRequired", "The email is required.");
    public static readonly Error PhoneNumberIsRequired = Error.Validation("User.PhoneNumberIsRequired", "The phone number is required.");
    public static readonly Error NameIsRequired = Error.Validation("User.NameIsRequired", "The name is required.");
    public static readonly Error PasswordIsRequired = Error.Validation("User.PasswordIsRequired", "The password is required.");
    public static readonly Error CannotAssignRoleToAttendee = Error.Validation("User.CannotAssignRoleToAttendee", "Cannot assign a role to an attendee.");
    public static readonly Error TenantIdRequired = Error.Validation("User.TenantIdRequired", "Tenant ID is required for this operation.");
} 