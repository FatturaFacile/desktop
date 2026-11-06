// <copyright file="PurchaseInvoiceImporter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Purchasing;

/// <summary>
/// Creates draft supplier invoices (and missing suppliers) from received FatturaPA files.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="signedDocumentReader">Extracts the XML from signed P7M files.</param>
public sealed class PurchaseInvoiceImporter(IUnitOfWork unitOfWork, ISignedDocumentReader signedDocumentReader)
{
    /// <summary>
    /// Imports a file.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <param name="content">The file content (XML or P7M).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<PurchaseImportResult> ImportAsync(string fileName, ReadOnlyMemory<byte> content, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        return ImportInternalAsync(fileName, FatturaPaPurchaseParser.Parse(signedDocumentReader.Unwrap(content)), cancellationToken);
    }

    private static string Normalize(string? value) =>
        string.Concat((value ?? string.Empty).Where(char.IsAsciiLetterOrDigit)).ToUpperInvariant();

    private static bool SameSupplier(Contact existing, Contact parsed)
    {
        var vat = Normalize(parsed.VatNumber);
        var fiscalCode = Normalize(parsed.FiscalCode);
        string[] vatForms = [vat, Normalize(parsed.Country) + vat];
        var sameVat = vat.Length > 0 && vatForms.Contains(Normalize(existing.VatNumber), StringComparer.Ordinal);

        return sameVat || (fiscalCode.Length > 0 && string.Equals(Normalize(existing.FiscalCode), fiscalCode, StringComparison.Ordinal));
    }

    private async Task<PurchaseImportResult> ImportInternalAsync(string fileName, IReadOnlyList<ParsedPurchaseInvoice> invoices, CancellationToken cancellationToken)
    {
        var contacts = unitOfWork.Repository<Contact>();
        var purchases = unitOfWork.Repository<PurchaseInvoice>();
        var lines = unitOfWork.Repository<PurchaseInvoiceLine>();
        var known = (await contacts.FindAsync(c => c.DeletedAt == null, cancellationToken).ConfigureAwait(false)).ToList();
        var created = 0;
        var skipped = 0;

        foreach (var parsed in invoices)
        {
            var supplier = known.FirstOrDefault(c => SameSupplier(c, parsed.Supplier));

            if (supplier is null)
            {
                supplier = parsed.Supplier;
                supplier.Id = Guid.NewGuid();
                await contacts.AddAsync(supplier, cancellationToken).ConfigureAwait(false);
                known.Add(supplier);
            }

            var supplierId = supplier.Id;
            var invoice = parsed.Invoice;
            var existing = await purchases.FindAsync(p => p.SupplierId == supplierId, cancellationToken).ConfigureAwait(false);

            if (existing.Any(p => p.DocumentDate.Date == invoice.DocumentDate.Date && string.Equals(p.DocumentNumber, invoice.DocumentNumber, StringComparison.OrdinalIgnoreCase)))
            {
                skipped++;
                continue;
            }

            invoice.Id = Guid.NewGuid();
            invoice.SupplierId = supplierId;
            invoice.Notes = $"Fattura elettronica {parsed.DocumentType} importata dal file {fileName}";

            foreach (var line in parsed.Lines)
            {
                line.Id = Guid.NewGuid();
                line.PurchaseInvoiceId = invoice.Id;
            }

            PurchaseInvoiceCalculator.Compute(invoice, [.. parsed.Lines]);
            await purchases.AddAsync(invoice, cancellationToken).ConfigureAwait(false);

            foreach (var line in parsed.Lines)
            {
                await lines.AddAsync(line, cancellationToken).ConfigureAwait(false);
            }

            created++;
        }

        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new PurchaseImportResult(created, skipped);
    }
}
