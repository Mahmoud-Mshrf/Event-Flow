using EventFlow.Domain.Common.Errors;

namespace EventFlow.Domain.Tenants;

public static class TenantErrors
{
    public static readonly Error InvalidName =
        Error.Validation(
            "Tenant.InvalidName",
            "Tenant name is required, must be between 6 and 100 characters.");

    public static readonly Error InvalidDescription =
        Error.Validation(
            "Tenant.InvalidDescription",
            "Tenant description is invalid, must be between 6 and 500 characters.");

    public static readonly Error NameAlreadyInUse =
    Error.Conflict("Tenant.NameAlreadyInUse",
        "An organization with this name already exists.");
}
