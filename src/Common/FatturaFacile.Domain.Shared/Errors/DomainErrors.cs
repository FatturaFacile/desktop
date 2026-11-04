// <copyright file="DomainErrors.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Shared.Errors;

/// <summary>
/// Provides strongly-typed domain errors categorized by bounded context and domain aggregate.
/// </summary>
public static class DomainErrors
{
    /// <summary>
    /// Contains error definitions specific to the Invoicing bounded context.
    /// </summary>
    public static class Invoice
    {
        /// <summary>
        /// Indicates that the requested invoice was not found.
        /// </summary>
        public static readonly DomainErrorDefinition NotFound = new("Invoice.NotFound", "The requested invoice could not be found.");

        /// <summary>
        /// Indicates that the invoice has already been fully settled/paid.
        /// </summary>
        public static readonly DomainErrorDefinition AlreadyPaid = new("Invoice.AlreadyPaid", "The invoice has already been fully paid.");

        /// <summary>
        /// Indicates that an invoice with the specified number already exists for the tenant.
        /// </summary>
        public static readonly DomainErrorDefinition NumberAlreadyExists = new("Invoice.NumberAlreadyExists", "An invoice with this number already exists for this tenant.");

        /// <summary>
        /// Indicates that an invoice cannot be cancelled in its current state.
        /// </summary>
        public static readonly DomainErrorDefinition CannotCancel = new("Invoice.CannotCancel", "Cannot cancel an invoice that is already fully paid. Issue a Credit Note instead.");
    }

    /// <summary>
    /// Contains error definitions specific to the Identity and Access bounded context.
    /// </summary>
    public static class Identity
    {
        /// <summary>
        /// Indicates that the provided credentials (email/password) are incorrect.
        /// </summary>
        public static readonly DomainErrorDefinition InvalidCredentials = new("Identity.InvalidCredentials", "Invalid email or password.");

        /// <summary>
        /// Indicates that the user account is locked out due to excessive failed login attempts.
        /// </summary>
        public static readonly DomainErrorDefinition UserLockedOut = new("Identity.UserLockedOut", "User account is locked due to multiple failed login attempts. Please try again later.");

        /// <summary>
        /// Indicates that the user account is inactive or disabled.
        /// </summary>
        public static readonly DomainErrorDefinition InactiveUser = new("Identity.InactiveUser", "User account is inactive. Please contact your system administrator.");

        /// <summary>
        /// Indicates that the tenant account is inactive or disabled.
        /// </summary>
        public static readonly DomainErrorDefinition InactiveTenant = new("Identity.InactiveTenant", "The tenant account is disabled.");

        /// <summary>
        /// Indicates that the specified tenant was not found.
        /// </summary>
        public static readonly DomainErrorDefinition TenantNotFound = new("Identity.TenantNotFound", "The specified tenant does not exist.");

        /// <summary>
        /// Indicates that the specified user was not found.
        /// </summary>
        public static readonly DomainErrorDefinition UserNotFound = new("Identity.UserNotFound", "The specified user does not exist.");

        /// <summary>
        /// Indicates that a user with the specified email address already exists.
        /// </summary>
        public static readonly DomainErrorDefinition EmailAlreadyExists = new("Identity.EmailAlreadyExists", "A user with this email address already exists.");

        /// <summary>
        /// Indicates that the provided refresh token is invalid, expired, or revoked.
        /// </summary>
        public static readonly DomainErrorDefinition InvalidRefreshToken = new("Identity.InvalidRefreshToken", "The provided refresh token is invalid, expired, or has been revoked.");
    }

    /// <summary>
    /// Contains error definitions related to multi-tenancy constraints and isolation.
    /// </summary>
    public static class Tenant
    {
        /// <summary>
        /// Indicates that a valid tenant context is missing from the request execution.
        /// </summary>
        public static readonly DomainErrorDefinition MissingContext = new("Tenant.MissingContext", "A valid tenant context is required for this operation.");

        /// <summary>
        /// Indicates that access to the requested tenant resource is unauthorized.
        /// </summary>
        public static readonly DomainErrorDefinition AccessDenied = new("Tenant.AccessDenied", "Access to the requested tenant resource is denied.");

        /// <summary>
        /// Indicates a mismatch between the current tenant context and the entity's tenant identifier.
        /// </summary>
        public static readonly DomainErrorDefinition Mismatch = new("Tenant.Mismatch", "Tenant identifier mismatch on entity mutation.");
    }

    /// <summary>
    /// Contains shared common validation and domain error definitions.
    /// </summary>
    public static class ValidationErrors
    {
        /// <summary>
        /// Indicates that one or more cross-cutting domain or input validation rules failed.
        /// </summary>
        public static readonly DomainErrorDefinition ValidationFailed = new("ValidationErrors.ValidationFailed", "One or more validation errors occurred.");
    }
}
