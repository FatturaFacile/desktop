// <copyright file="Contact.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities.Crm;

/// <summary>
/// Represents an individual business representative, account executive, or billing contact associated with a client company.
/// </summary>
public sealed class Contact : Entity<Guid>
{

    /// <summary>
    /// Initializes a new instance of the <see cref="Contact"/> class.
    /// </summary>
    /// <param name="clientId">The unique identifier of the parent client entity.</param>
    /// <param name="firstName">The given name of the contact person.</param>
    /// <param name="lastName">The family name or surname of the contact person.</param>
    /// <param name="email">The direct electronic mail address.</param>
    /// <param name="phone">The direct or mobile telephone contact number.</param>
    /// <param name="role">The business role or functional title within the client organization.</param>
    /// <param name="isPrimary">Specifies whether this contact is designated as the primary liaison for invoicing and notices.</param>
    internal Contact(
        Guid    clientId,
        string  firstName,
        string  lastName,
        string  email,
        string? phone,
        string? role,
        bool    isPrimary)
    {
        Id          = Guid.NewGuid();
        ClientId    = clientId;
        FirstName   = firstName;
        LastName    = lastName;
        Email       = email;
        PhoneNumber = phone;
        Role        = role;
        IsPrimary   = isPrimary;
    }

    /// <summary>
    ///
    /// </summary>
    private Contact()
    {
    }

    /// <summary>
    /// Gets the unique identifier of the parent client entity to which this contact belongs.
    /// </summary>
    public Guid ClientId { get; }

    /// <summary>
    /// Gets the given name of the contact person.
    /// </summary>
    public string FirstName { get; } = string.Empty;

    /// <summary>
    /// Gets the surname or family name of the contact person.
    /// </summary>
    public string LastName { get; } = string.Empty;

    /// <summary>
    /// Gets the primary electronic mail address of the contact person.
    /// </summary>
    public string Email { get; } = string.Empty;

    /// <summary>
    /// Gets the direct telephone or mobile contact number, if available.
    /// </summary>
    public string? PhoneNumber { get; }

    /// <summary>
    /// Gets the professional role or department of the contact person (e.g., "Accounts Payable", "Purchasing Agent").
    /// </summary>
    public string? Role { get; }

    /// <summary>
    /// Gets a value indicating whether this individual is the primary communication contact for billing, payment reminders, and contract notices.
    /// </summary>
    public bool IsPrimary { get; }

}
