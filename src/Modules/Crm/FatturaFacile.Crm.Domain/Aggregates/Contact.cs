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
    /// <param name="request">The request containing the contact details.</param>
    /// <returns>A new <see cref="Contact"/> instance.</returns>
    public static Contact Create(CreateContactRequest request)
    {
       ArgumentNullException.ThrowIfNull(request);

       return new Contact
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            Name = request.Name.Trim(),
            ContactType = request.ContactType,
            CountryCode = request.CountryCode.ToUpperInvariant(),
            VatNumber = request.VatNumber?.Trim(),
            TaxCode = request.TaxCode?.Trim(),
            Email = request.Email?.Trim(),
            SdiCode = request.SdiCode?.Trim()
                   ?? (string.Equals(request.CountryCode, "IT", StringComparison.OrdinalIgnoreCase) ? "0000000" : null),
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }
}
