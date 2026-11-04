namespace FatturaFacile.Domain.Shared.Errors;

public static class DomainErrors
{
    public static class Invoice
    {
        public static readonly DomainErrorDefinition NotFound = new("Invoice.NotFound", "The requested invoice could not be found.");
        public static readonly DomainErrorDefinition AlreadyPaid = new("Invoice.AlreadyPaid", "The invoice has already been fully paid.");
        public static readonly DomainErrorDefinition NumberAlreadyExists = new("Invoice.NumberAlreadyExists", "An invoice with this number already exists for this tenant.");
        public static readonly DomainErrorDefinition CannotCancel = new("Invoice.CannotCancel", "Cannot cancel an invoice that is already fully paid. Issue a Credit Note instead.");
    }

    public static class Identity
    {
        public static readonly DomainErrorDefinition InvalidCredentials = new("Identity.InvalidCredentials", "Invalid email or password.");
        public static readonly DomainErrorDefinition UserLockedOut = new("Identity.UserLockedOut", "User account is locked due to multiple failed login attempts. Please try again later.");
        public static readonly DomainErrorDefinition InactiveUser = new("Identity.InactiveUser", "User account is inactive. Please contact your system administrator.");
        public static readonly DomainErrorDefinition InactiveTenant = new("Identity.InactiveTenant", "The tenant account is disabled.");
        public static readonly DomainErrorDefinition TenantNotFound = new("Identity.TenantNotFound", "The specified tenant does not exist.");
        public static readonly DomainErrorDefinition UserNotFound = new("Identity.UserNotFound", "The specified user does not exist.");
        public static readonly DomainErrorDefinition EmailAlreadyExists = new("Identity.EmailAlreadyExists", "A user with this email address already exists.");
        public static readonly DomainErrorDefinition InvalidRefreshToken = new("Identity.InvalidRefreshToken", "The provided refresh token is invalid, expired, or has been revoked.");
    }

    public static class Tenant
    {
        public static readonly DomainErrorDefinition MissingContext = new("Tenant.MissingContext", "A valid tenant context is required for this operation.");
        public static readonly DomainErrorDefinition AccessDenied = new("Tenant.AccessDenied", "Access to the requested tenant resource is denied.");
        public static readonly DomainErrorDefinition Mismatch = new("Tenant.Mismatch", "Tenant identifier mismatch on entity mutation.");
    }

    public static class Common
    {
        public static readonly DomainErrorDefinition ValidationFailed = new("Common.ValidationFailed", "One or more validation errors occurred.");
    }
}
