// <copyright file="EraseContactDataCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Privacy;

/// <summary>
/// Handles <see cref="EraseContactDataCommand"/>. Accounting documents must be kept for 10 years after the end of the
/// year (art. 958f CO, art. 2220 c.c.): until then only the contact data not required by them are erased.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class EraseContactDataCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<EraseContactDataCommand, string>
{
    private const int RetentionYears = 10;

    /// <inheritdoc />
    public Task<string> Handle(EraseContactDataCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.ContactId, cancellationToken);
    }

    private async Task<string> HandleInternalAsync(Guid contactId, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.Repository<Contact>();
        var contact = await repository.GetByIdAsync(contactId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Contatto non trovato.");
        var invoices = await unitOfWork.Repository<Invoice>().FindAsync(i => i.ContactId == contactId, cancellationToken).ConfigureAwait(false);
        var purchases = await unitOfWork.Repository<PurchaseInvoice>().FindAsync(p => p.SupplierId == contactId, cancellationToken).ConfigureAwait(false);
        var lastDocument = invoices.Select(static i => i.Date).Concat(purchases.Select(static p => p.DocumentDate)).DefaultIfEmpty(DateTime.MinValue).Max();
        var keepUntil = lastDocument == DateTime.MinValue ? DateTime.MinValue : new DateTime(lastDocument.Year + RetentionYears, 12, 31, 0, 0, 0, DateTimeKind.Unspecified);

        contact.Email = null;
        contact.Phone = null;
        contact.Iban = null;
        contact.QrIban = null;
        string message;

        if (keepUntil >= DateTime.Today)
        {
            message = string.Create(
                CultureInfo.InvariantCulture,
                $"E-mail, telefono e coordinate bancarie cancellati. Anagrafica e dati fiscali restano fino al {keepUntil:dd.MM.yyyy} perché figurano in documenti contabili da conservare (art. 958f CO, art. 2220 c.c.).");
        }
        else
        {
            contact.CompanyName = "Contatto anonimizzato";
            contact.FirstName = null;
            contact.LastName = null;
            contact.Street = null;
            contact.HouseNumber = null;
            contact.PostalCode = null;
            contact.City = null;
            contact.Province = null;
            contact.VatNumber = null;
            contact.FiscalCode = null;
            contact.UidNumber = null;
            contact.SdiCode = null;
            contact.Pec = null;
            contact.DeletedAt = DateTime.UtcNow;
            message = "Dati del contatto anonimizzati e contatto rimosso dagli elenchi.";
        }

        repository.Update(contact);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return message;
    }
}
