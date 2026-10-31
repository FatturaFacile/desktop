// <copyright file="VatCode.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Represents a fiscal value-added tax (VAT) code definition, standard rate, or tax exemption scheme.
/// </summary>
public class VatCode : EntityBase, IMustHaveTenant
{
    /// <inheritdoc />
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the unique short identifier or mnemonic code (e.g. "IVA22", "MWST81", "EXEMPT").
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the descriptive title explaining the tax rate or legal exemption article.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the numeric percentage rate (e.g. 22.0 for 22%, 8.1 for 8.1%, 0.0 for exempt).
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>
    /// Gets or sets the two-letter ISO country code governing this tax regulation (e.g. "CH", "IT").
    /// </summary>
    public string CountryIso { get; set; } = "CH";

    /// <summary>
    /// Gets or sets a value indicating whether this code represents a VAT reverse charge transaction.
    /// </summary>
    public bool IsReverseCharge { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this code triggers Italian split payment fiscal handling.
    /// </summary>
    public bool IsSplitPayment { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this VAT code is available for invoice selection.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the navigation property for the owning tenant organization.
    /// </summary>
    public Tenant? Tenant { get; set; }
}
