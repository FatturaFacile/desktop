// <copyright file="Contact.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Shared.Primitives;

namespace FatturaFacile.Crm.Domain.Aggregates;

/// <summary>
/// Represents a customer, supplier, or business partner contact aggregate root.
/// </summary>
public sealed class Contact : AggregateRoot<Guid>, IHasTenant, ISoftDeletable, IAuditableEntity
{
    private Contact()
    {
    }

    /// <inheritdoc />
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Gets the legal or primary registered entity name of the contact.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the optional commercial or DBA trade name of the contact.
    /// </summary>
    public string? TradeName { get; }

    /// <summary>
    /// Gets the classification role of the contact (Customer, Supplier, Lead).
    /// </summary>
    public ContactType ContactType { get; private set; }

    /// <summary>
    /// Gets the value-added tax (VAT) registration number (e.g., "IT12345678901").
    /// </summary>
    public string? VatNumber { get; private set; }

    /// <summary>
    /// Gets the national fiscal or tax identification code (e.g., Codice Fiscale).
    /// </summary>
    public string? TaxCode { get; private set; }

    /// <summary>
    /// Gets the primary electronic mail address.
    /// </summary>
    public string? Email { get; private set; }

    /// <summary>
    /// Gets the primary telephone contact number.
    /// </summary>
    public string? Phone { get; }

    /// <summary>
    /// Gets the certified electronic mail address (Posta Elettronica Certificata - PEC) for Italian e-invoicing.
    /// </summary>
    public string? Pec { get; }

    /// <summary>
    /// Gets the 7-character electronic invoicing routing recipient code (Codice Destinatario SDI).
    /// </summary>
    public string? SdiCode { get; private set; }

    /// <summary>
    /// Gets the 2-letter ISO 3166-1 alpha-2 country code (e.g., "IT", "CH", "DE").
    /// </summary>
    public string CountryCode { get; private set; } = "IT";

    /// <inheritdoc />
    public bool IsDeleted { get; }

    /// <inheritdoc />
    public DateTimeOffset? DeletedAt { get;  }

    /// <inheritdoc />
    public DateTimeOffset CreatedAt { get; private set; }

    /// <inheritdoc />
    public string? CreatedBy { get; }

    /// <inheritdoc />
    public DateTimeOffset? UpdatedAt { get;  }

    /// <inheritdoc />
    public string? UpdatedBy { get; }

    /// <summary>
    /// Factory method to create a new validated <see cref="Contact"/> aggregate root.
    /// </summary>
    /// <param name="tenantId">The owning tenant identifier.</param>
    /// <param name="name">The legal name of the contact.</param>
    /// <param name="type">The contact type classification.</param>
    /// <param name="countryCode">The 2-letter ISO country code (defaults to "IT").</param>
    /// <param name="vatNumber">The optional VAT number.</param>
    /// <param name="taxCode">The optional tax code.</param>
    /// <param name="email">The optional email address.</param>
    /// <param name="sdiCode">The optional SDI recipient code.</param>
    /// <returns>A new <see cref="Contact"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="tenantId"/> is empty or <paramref name="name"/> is null or empty.</exception>
    public static Contact Create(
        Guid tenantId,
        string name,
        ContactType type,
        string countryCode = "IT",
        string? vatNumber = null,
        string? taxCode = null,
        string? email = null,
        string? sdiCode = null)
    {
        if (tenantId == Guid.Empty)
        {
            return throw new ArgumentException("Tenant ID is required.", nameof(tenantId));
        }
        else if (string.IsNullOrWhiteSpace(name))
        {
            return throw new ArgumentException("Contact name is required.", nameof(name));
        }
        else
        {
            return new Contact
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = name.Trim(),
                ContactType = type,
                CountryCode = countryCode.ToUpperInvariant(),
                VatNumber = vatNumber?.Trim(),
                TaxCode = taxCode?.Trim(),
                Email = email?.Trim(),
                SdiCode = sdiCode?.Trim() ?? (string.Equals(countryCode, "IT", StringComparison.OrdinalIgnoreCase) ? "0000000" : null),
                CreatedAt = DateTimeOffset.UtcNow,
            };
        }
    }
}
