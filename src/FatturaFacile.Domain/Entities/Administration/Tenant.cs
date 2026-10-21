// <copyright file="Tenant.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Administration;

/// <summary>
/// Represents tenant.
/// </summary>
public sealed class Tenant : AggregateRoot<Guid>, IAuditableEntity
{
    private readonly List<Branch> branches = [];

    private Tenant()
    {
    }

    /// <summary>
    /// Gets the e.g. &amp;quot;TENANT-IT-01&amp;quot;, &amp;quot;TENANT-CH-01&amp;quot;.
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the business name.
    /// </summary>
    public string BusinessName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the &amp;quot;IT&amp;quot; or &amp;quot;CH&amp;quot;.
    /// </summary>
    public string CountryCode { get; private set; } = "IT";

    /// <summary>
    /// Gets the base currency.
    /// </summary>
    public Currency BaseCurrency { get; private set; } = Currency.EUR;

    /// <summary>
    /// Gets the vat number.
    /// </summary>
    public string? VatNumber { get; private set; }

    /// <summary>
    /// Gets the fiscal code.
    /// </summary>
    public string? FiscalCode { get; private set; }

    /// <summary>
    /// Gets the sdi code.
    /// </summary>
    public string? SdiCode { get; private set; }

    /// <summary>
    /// Gets the swiss uid.
    /// </summary>
    public string? SwissUid { get; private set; }

    /// <summary>
    /// Gets the qr iban.
    /// </summary>
    public string? QrIban { get; private set; }

    /// <summary>
    /// Gets the contact email.
    /// </summary>
    public string ContactEmail { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether active.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Gets the branches.
    /// </summary>
    public IReadOnlyCollection<Branch> Branches => branches.AsReadOnly();

    /// <summary>
    /// Gets or sets the created at utc.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the created by.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the updated at utc.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the updated by.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Create.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <param name="businessName">The business name.</param>
    /// <param name="countryCode">The country code.</param>
    /// <param name="baseCurrency">The base currency.</param>
    /// <param name="contactEmail">The contact email.</param>
    /// <param name="vatNumber">The vat number.</param>
    /// <param name="fiscalCode">The fiscal code.</param>
    /// <param name="sdiCode">The sdi code.</param>
    /// <param name="swissUid">The swiss uid.</param>
    /// <param name="qrIban">The qr iban.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static Tenant Create(
        string code,
        string businessName,
        string countryCode,
        Currency baseCurrency,
        string contactEmail,
        string? vatNumber = null,
        string? fiscalCode = null,
        string? sdiCode = null,
        string? swissUid = null,
        string? qrIban = null)
    {
        return new Tenant
        {
            Id = Guid.NewGuid(),
            Code = code?.Trim().ToUpperInvariant() ?? string.Empty,
            BusinessName = businessName?.Trim() ?? string.Empty,
            CountryCode = countryCode?.Trim().ToUpperInvariant() ?? string.Empty,
            BaseCurrency = baseCurrency,
            ContactEmail = contactEmail?.Trim() ?? string.Empty,
            VatNumber = vatNumber?.Trim() ?? string.Empty,
            FiscalCode = fiscalCode?.Trim() ?? string.Empty,
            SdiCode = sdiCode?.Trim() ?? string.Empty,
            SwissUid = swissUid?.Trim() ?? string.Empty,
            QrIban = qrIban?.Trim() ?? string.Empty,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Add Branch.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <param name="name">The name.</param>
    /// <param name="address">The address.</param>
    /// <param name="isHeadquarters">The is headquarters.</param>
    public void AddBranch(
        string code,
        string name,
        Address address,
        bool isHeadquarters = false) =>
        branches.Add(
            new Branch(
                Id,
                code ?? string.Empty,
                name ?? string.Empty,
                address,
                isHeadquarters));
}
