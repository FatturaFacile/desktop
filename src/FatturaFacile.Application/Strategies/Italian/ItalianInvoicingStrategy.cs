// <copyright file="ItalianInvoicingStrategy.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Strategies.Italian;

/// <summary>
/// Italian invoicing strategy: validates the data and generates the FatturaPA XML file ready for transmission to the
/// Sistema di Interscambio (SDI). Transmission itself (via an accredited intermediary, web service or PEC) is a separate step.
/// </summary>
public sealed class ItalianInvoicingStrategy : ICountryInvoicingStrategy
{
    /// <inheritdoc/>
    public string CountryCode => "IT";

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

        var stampDuty = FatturaPaRules.ComputeStampDuty(invoice.Items);

        if (stampDuty > 0m)
        {
            invoice.StampDutyApplied = true;
            invoice.StampDutyAmount = stampDuty;
        }

        var errors = FatturaPaValidator.Validate(invoice, tenant, contact);

        var progressive = invoice.SequentialNumber is { } sequential
            ? sequential.ToString("D5", CultureInfo.InvariantCulture)
            : invoice.Id.ToString("N", CultureInfo.InvariantCulture)[^5..].ToUpperInvariant();

        var format = contact.IsPublicAdministration ? "FPA12" : "FPR12";
        var fileName = FatturaPaXmlBuilder.BuildFileName(tenant, progressive);
        string? xml = null;

        if (errors.Count == 0)
        {
            var document = FatturaPaXmlBuilder.Build(invoice, tenant, contact, progressive);
            xml = FatturaPaXmlBuilder.Serialize(document);
        }

        invoice.SdiMetadata ??= new InvoiceSdiMetadata
        {
            InvoiceId = invoice.Id,
            TenantId = invoice.TenantId,
            CreatedAt = DateTime.UtcNow,
        };

        var metadata = invoice.SdiMetadata;
        metadata.TransmissionFormat = format;
        metadata.FileName = fileName;
        metadata.XmlContent = xml;
        metadata.ValidationErrors = errors.Count == 0 ? null : string.Join(Environment.NewLine, errors);

        // Never fake an SDI outcome: the identifier and the delivery state are set only by the SDI receipts.
        metadata.SdiStatus = errors.Count == 0 ? "generated" : "invalid";
        metadata.SdiIdentifier = null;
        metadata.SentAt = null;
        metadata.UpdatedAt = DateTime.UtcNow;

        return Task.CompletedTask;
    }
}
