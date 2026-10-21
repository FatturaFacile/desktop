// <copyright file="Client.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Crm;

/// <summary>
/// Represents client.
/// </summary>
public sealed class Client : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    private readonly List<Contact> contacts = [];

    private Client()
    {
    }

    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the business name.
    /// </summary>
    public string BusinessName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the trade name.
    /// </summary>
    public string? TradeName { get; }

    /// <summary>
    /// Gets the jurisdiction.
    /// </summary>
    public Jurisdiction Jurisdiction { get; private set; }

    /// <summary>
    /// Gets the italian Fiscal Details.
    /// </summary>
    public FiscalCode? FiscalCode { get; private set; }

    /// <summary>
    /// Gets the vat number.
    /// </summary>
    public VatNumber? VatNumber { get; private set; }

    /// <summary>
    /// Gets the 7-character SDI recipient code or 0000000.
    /// </summary>
    public string? SdiCode { get; private set; }

    /// <summary>
    /// Gets the pec email.
    /// </summary>
    public string? PecEmail { get; private set; }

    /// <summary>
    /// Gets the swiss Fiscal Details.
    /// </summary>
    public string? SwissUid { get; private set; } // CHE-xxx.xxx.xxx

    /// <summary>
    /// Gets the qr iban.
    /// </summary>
    public QrIban? QrIban { get; private set; }

    /// <summary>
    /// Gets the tI, GR, ZH, etc.
    /// </summary>
    public string? CantonalCode { get; private set; }

    /// <summary>
    /// Gets the contact &amp; Commercial.
    /// </summary>
    public Address Address { get; private set; } = null!;

    /// <summary>
    /// Gets the email.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the phone number.
    /// </summary>
    public string? PhoneNumber { get; private set; }

    /// <summary>
    /// Gets the payment term days.
    /// </summary>
    public int PaymentTermDays { get; private set; } = 30;

    /// <summary>
    /// Gets the credit limit.
    /// </summary>
    public Money CreditLimit { get; private set; } = Money.Euro(5000);

    /// <summary>
    /// Gets a value indicating whether active.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Gets the contacts.
    /// </summary>
    public IReadOnlyCollection<Contact> Contacts => contacts.AsReadOnly();

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
    /// Create Italian.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="businessName">The business name.</param>
    /// <param name="fiscalCode">The fiscal code.</param>
    /// <param name="vatNumber">The vat number.</param>
    /// <param name="sdiCode">The sdi code.</param>
    /// <param name="pecEmail">The pec email.</param>
    /// <param name="address">The address.</param>
    /// <param name="email">The email.</param>
    /// <param name="phone">The phone.</param>
    /// <param name="paymentTermDays">The payment term days.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static Client CreateItalian(
        Guid tenantId,
        string businessName,
        FiscalCode fiscalCode,
        VatNumber vatNumber,
        string sdiCode,
        string pecEmail,
        Address address,
        string email,
        string? phone = null,
        int paymentTermDays = 30)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(businessName);
        return new Client
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BusinessName = businessName.Trim(),
            Jurisdiction = Jurisdiction.Italy,
            FiscalCode = fiscalCode,
            VatNumber = vatNumber,
            SdiCode = string.IsNullOrWhiteSpace(sdiCode) ? "0000000" : sdiCode.Trim().ToUpperInvariant(),
            PecEmail = pecEmail?.Trim(),
            Address = address,
            Email = email?.Trim() ?? string.Empty,
            PhoneNumber = phone?.Trim(),
            PaymentTermDays = paymentTermDays,
            CreditLimit = Money.Euro(10000),
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Create Swiss.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="businessName">The business name.</param>
    /// <param name="swissUid">The swiss uid.</param>
    /// <param name="cantonalCode">The cantonal code.</param>
    /// <param name="qrIban">The qr iban.</param>
    /// <param name="address">The address.</param>
    /// <param name="email">The email.</param>
    /// <param name="phone">The phone.</param>
    /// <param name="paymentTermDays">The payment term days.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static Client CreateSwiss(
        Guid tenantId,
        string businessName,
        string swissUid,
        string cantonalCode,
        QrIban? qrIban,
        Address address,
        string email,
        string? phone = null,
        int paymentTermDays = 30)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(businessName);
        return new Client
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BusinessName = businessName.Trim(),
            Jurisdiction = Jurisdiction.Switzerland,
            SwissUid = swissUid?.Trim().ToUpperInvariant(),
            CantonalCode = cantonalCode?.Trim().ToUpperInvariant(),
            QrIban = qrIban,
            Address = address,
            Email = email?.Trim() ?? string.Empty,
            PhoneNumber = phone?.Trim() ?? string.Empty,
            PaymentTermDays = paymentTermDays,
            CreditLimit = Money.Chf(15000),
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Add Contact.
    /// </summary>
    /// <param name="firstName">The first name.</param>
    /// <param name="lastName">The last name.</param>
    /// <param name="email">The email.</param>
    /// <param name="phone">The phone.</param>
    /// <param name="role">The role.</param>
    /// <param name="isPrimary">The is primary.</param>
    public void AddContact(
        string firstName,
        string lastName,
        string email,
        string? phone,
        string? role,
        bool isPrimary)
    {
        var contact = new Contact(
            Id,
            firstName,
            lastName,
            email,
            phone,
            role,
            isPrimary);
        contacts.Add(contact);
    }

    /// <summary>
    /// Update Commercial Terms.
    /// </summary>
    /// <param name="paymentTermDays">The payment term days.</param>
    /// <param name="creditLimit">The credit limit.</param>
    public void UpdateCommercialTerms(
        int paymentTermDays,
        Money creditLimit)
    {
        PaymentTermDays = paymentTermDays;
        CreditLimit = creditLimit;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivate.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
