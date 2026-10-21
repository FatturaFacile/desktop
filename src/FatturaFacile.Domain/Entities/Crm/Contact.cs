// <copyright file="Contact.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities.Crm;

/// <summary>
/// Represents contact.
/// </summary>
public sealed class Contact : Entity<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Contact"/> class.
    /// </summary>
    /// <param name="clientId">The client id.</param>
    /// <param name="firstName">The first name.</param>
    /// <param name="lastName">The last name.</param>
    /// <param name="email">The email.</param>
    /// <param name="phone">The phone.</param>
    /// <param name="role">The role.</param>
    /// <param name="isPrimary">The is primary.</param>
    internal Contact(
        Guid clientId,
        string firstName,
        string lastName,
        string email,
        string? phone,
        string? role,
        bool isPrimary)
    {
        Id = Guid.NewGuid();
        ClientId = clientId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phone;
        Role = role;
        IsPrimary = isPrimary;
    }

    private Contact()
    {
    }

    /// <summary>
    /// Gets the client id.
    /// </summary>
    public Guid ClientId { get; }

    /// <summary>
    /// Gets the first name.
    /// </summary>
    public string FirstName { get; } = string.Empty;

    /// <summary>
    /// Gets the last name.
    /// </summary>
    public string LastName { get; } = string.Empty;

    /// <summary>
    /// Gets the email.
    /// </summary>
    public string Email { get; } = string.Empty;

    /// <summary>
    /// Gets the phone number.
    /// </summary>
    public string? PhoneNumber { get; }

    /// <summary>
    /// Gets the role.
    /// </summary>
    public string? Role { get; }

    /// <summary>
    /// Gets a value indicating whether primary.
    /// </summary>
    public bool IsPrimary { get; }
}
