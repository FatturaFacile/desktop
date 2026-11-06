// <copyright file="ExportContactDataQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text;
using System.Text.Json;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Models;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Privacy;

/// <summary>
/// Handles <see cref="ExportContactDataQuery"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class ExportContactDataQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<ExportContactDataQuery, GeneratedFile>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <inheritdoc />
    public Task<GeneratedFile> Handle(ExportContactDataQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.ContactId, cancellationToken);
    }

    private async Task<GeneratedFile> HandleInternalAsync(Guid contactId, CancellationToken cancellationToken)
    {
        var contact = await unitOfWork.Repository<Contact>().GetByIdAsync(contactId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Contatto non trovato.");
        var invoices = await unitOfWork.Repository<Invoice>().FindAsync(i => i.ContactId == contactId, cancellationToken).ConfigureAwait(false);
        var purchases = await unitOfWork.Repository<PurchaseInvoice>().FindAsync(p => p.SupplierId == contactId, cancellationToken).ConfigureAwait(false);

        var export = new
        {
            ExportedAt = DateTime.Now,
            Contact = new
            {
                contact.Type,
                contact.CompanyName,
                contact.FirstName,
                contact.LastName,
                contact.Street,
                contact.HouseNumber,
                contact.PostalCode,
                contact.City,
                contact.Province,
                contact.Country,
                contact.VatNumber,
                contact.FiscalCode,
                contact.UidNumber,
                contact.SdiCode,
                contact.Pec,
                contact.Email,
                contact.Phone,
                contact.Iban,
                contact.PaymentTermsDays,
                contact.CreatedAt,
                contact.UpdatedAt,
            },
            Invoices = invoices.OrderBy(static i => i.Date).Select(static i => new { i.InvoiceNumber, i.Date, i.Currency, i.TotalGross, i.Status }),
            SupplierInvoices = purchases.OrderBy(static p => p.DocumentDate).Select(static p => new { p.DocumentNumber, p.DocumentDate, p.Currency, p.TotalGross, p.Status }),
        };

        var name = string.Concat(contact.DisplayName.Where(static c => char.IsLetterOrDigit(c) || c == '-'));
        return new GeneratedFile(
            string.Create(CultureInfo.InvariantCulture, $"dati-contatto-{name}-{DateTime.Today:yyyyMMdd}.json"),
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(export, JsonOptions)),
            string.Create(CultureInfo.InvariantCulture, $"Dati di {contact.DisplayName}: {invoices.Count} fatture, {purchases.Count} fatture fornitore."));
    }
}
