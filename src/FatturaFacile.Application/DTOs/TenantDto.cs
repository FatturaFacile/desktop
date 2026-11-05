// <copyright file="TenantDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer object representing a company tenant.
/// </summary>
public sealed class TenantDto
{
    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the trade name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the legal registered name.
    /// </summary>
    public string LegalName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Italian fiscal code.
    /// </summary>
    public string? FiscalCode { get; set; }

    /// <summary>
    /// Gets or sets the VAT number.
    /// </summary>
    public string? VatNumber { get; set; }

    /// <summary>
    /// Gets or sets the Swiss business identification number.
    /// </summary>
    public string? UidNumber { get; set; }

    /// <summary>
    /// Gets or sets the country code ("CH", "IT").
    /// </summary>
    public string Country { get; set; } = "IT";

    /// <summary>
    /// Gets or sets the default base currency ("CHF", "EUR").
    /// </summary>
    public string DefaultCurrency { get; set; } = "EUR";

    /// <summary>
    /// Gets or sets the bank IBAN.
    /// </summary>
    public string? Iban { get; set; }

    /// <summary>
    /// Gets or sets the Swiss QR-IBAN.
    /// </summary>
    public string? QrIban { get; set; }

    /// <summary>
    /// Gets or sets the SDI recipient code.
    /// </summary>
    public string? SdiCode { get; set; }

    /// <summary>
    /// Gets or sets the PEC certified email.
    /// </summary>
    public string? PecEmail { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether tenant is in maintenance.
    /// </summary>
    public bool IsMaintenance { get; set; }
}
