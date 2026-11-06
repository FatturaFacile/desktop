// <copyright file="SwissInvoicingStrategy.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Validation;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Strategies.Swiss;

/// <summary>
/// Swiss invoicing strategy producing the QR-bill payment part (Swiss Implementation Guidelines QR-bill, SPC 0200).
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SwissInvoicingStrategy"/> class.
/// </remarks>
/// <param name="qrCodeEncoder">The QR code encoder.</param>
public sealed class SwissInvoicingStrategy(IQrCodeEncoder qrCodeEncoder) : ICountryInvoicingStrategy
{
    /// <inheritdoc/>
    public string CountryCode => "CH";

    /// <summary>
    /// Builds the QR-bill request for an invoice.
    /// </summary>
    /// <param name="invoice">The invoice.</param>
    /// <param name="tenant">The issuing tenant (creditor).</param>
    /// <param name="contact">The customer (debtor).</param>
    /// <returns>The QR-bill request.</returns>
    public static SwissQrBillRequest CreateRequest(Invoice invoice, Tenant tenant, Contact contact)
    {
        ArgumentNullException.ThrowIfNull(invoice);
        ArgumentNullException.ThrowIfNull(tenant);
        ArgumentNullException.ThrowIfNull(contact);

        var account = IbanValidator.IsSwissQrIban(tenant.QrIban)
            ? IbanValidator.Normalize(tenant.QrIban)
            : IbanValidator.Normalize(tenant.Iban);

        var referenceType = IbanValidator.IsSwissQrIban(account) ? "QRR" : "SCOR";
        var reference = string.Equals(referenceType, "QRR", StringComparison.Ordinal)
            ? SwissPaymentReference.CreateQrReference(invoice.InvoiceNumber)
            : SwissPaymentReference.CreateCreditorReference(invoice.InvoiceNumber);

        SwissQrBillAddress creditor = new(
            string.IsNullOrWhiteSpace(tenant.LegalName) ? tenant.Name : tenant.LegalName,
            tenant.Street,
            tenant.HouseNumber,
            tenant.PostalCode ?? string.Empty,
            tenant.City ?? string.Empty,
            tenant.Country);

        SwissQrBillAddress? debtor = string.IsNullOrWhiteSpace(contact.PostalCode) || string.IsNullOrWhiteSpace(contact.City)
            ? null
            : new(
                contact.DisplayName,
                contact.Street,
                contact.HouseNumber,
                contact.PostalCode,
                contact.City,
                contact.Country);

        return new SwissQrBillRequest(
            account,
            creditor,
            invoice.TotalGross > 0 ? invoice.TotalGross : null,
            invoice.Currency,
            debtor,
            referenceType,
            reference,
            $"Fattura {invoice.InvoiceNumber}");
    }

    /// <inheritdoc/>
    public Task ProcessInvoiceComplianceAsync(
        Invoice invoice,
        Tenant tenant,
        Contact contact,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(invoice);
        ArgumentNullException.ThrowIfNull(tenant);
        ArgumentNullException.ThrowIfNull(contact);

        var request = CreateRequest(invoice, tenant, contact);
        var errors = SwissQrBillPayloadBuilder.Validate(request);

        string? payload = null;
        string? svg = null;

        if (errors.Count == 0)
        {
            payload = SwissQrBillPayloadBuilder.Build(request);
            svg = SwissQrBillSvgRenderer.Render(request, qrCodeEncoder.Encode(payload));
        }

        invoice.QrBillData ??= new InvoiceQrBillData
        {
            InvoiceId = invoice.Id,
            TenantId = invoice.TenantId,
            CreatedAt = DateTime.UtcNow,
        };

        var data = invoice.QrBillData;
        data.QrIban = request.Account;
        data.ReferenceType = request.ReferenceType;
        data.PaymentReference = request.Reference;
        data.CreditorInfo = string.Join(", ", request.Creditor.Name, request.Creditor.Street, request.Creditor.HouseNumber, request.Creditor.PostalCode, request.Creditor.Town, request.Creditor.Country);
        data.DebtorInfo = request.Debtor is null
            ? string.Empty
            : string.Join(", ", request.Debtor.Name, request.Debtor.Street, request.Debtor.HouseNumber, request.Debtor.PostalCode, request.Debtor.Town, request.Debtor.Country);
        data.QrPayload = payload;
        data.QrBillSvg = svg;
        data.ValidationErrors = errors.Count == 0 ? null : string.Join(Environment.NewLine, errors);
        data.UpdatedAt = DateTime.UtcNow;

        return Task.CompletedTask;
    }
}
