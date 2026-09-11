using EventFlow.Domain.Common.Errors;

namespace EventFlow.Domain.Users;

public static class UserErrors
{
    public static readonly Error InvalidPhoneNumber = Error.Validation("User.InvalidPhoneNumber", "The phone number is invalid.");
    public static readonly Error InvalidEmail = Error.Validation("User.InvalidEmail", "The email is invalid.");
    public static readonly Error InvalidPassword = Error.Validation("User.InvalidPassword", "The password is invalid.");
    public static readonly Error InvalidRole = Error.Validation("User.InvalidRole", "The role is invalid.");
    public static readonly Error InvalidName = Error.Validation("User.InvalidName", "The name is invalid.");
} 