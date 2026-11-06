// <copyright file="SavePurchaseInvoiceCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Records;
using FatturaFacile.Application.Purchasing;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Purchasing;

/// <summary>
/// Handler for <see cref="SavePurchaseInvoiceCommand"/>: only drafts can be changed.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SavePurchaseInvoiceCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class SavePurchaseInvoiceCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<SavePurchaseInvoiceCommand, Guid>
{
    /// <inheritdoc/>
    public Task<Guid> Handle(SavePurchaseInvoiceCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<Guid> HandleInternalAsync(SavePurchaseInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoices = unitOfWork.Repository<PurchaseInvoice>();
        var lineRepository = unitOfWork.Repository<PurchaseInvoiceLine>();
        var invoice = request.Invoice.Id == Guid.Empty ? null : await invoices.GetByIdAsync(request.Invoice.Id, cancellationToken).ConfigureAwait(false);

        if (invoice is null)
        {
            invoice = RecordCopier.Clone(request.Invoice);
            invoice.Id = Guid.NewGuid();
            invoice.Status = "Draft";
            invoice.PaidAmount = 0m;
            invoice.JournalEntryId = null;
            await invoices.AddAsync(invoice, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            if (!string.Equals(invoice.Status, "Draft", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("La fattura fornitore è già registrata: non può essere modificata.");
            }

            RecordCopier.CopyValues(request.Invoice, invoice);
            invoice.Status = "Draft";
            invoices.Update(invoice);

            var invoiceId = invoice.Id;
            var oldLines = await lineRepository.FindAsync(l => l.PurchaseInvoiceId == invoiceId, cancellationToken).ConfigureAwait(false);

            foreach (var old in oldLines)
            {
                lineRepository.Delete(old);
            }
        }

        List<PurchaseInvoiceLine> lines = [.. request.Lines.Select(RecordCopier.Clone)];

        foreach (var line in lines)
        {
            line.Id = Guid.NewGuid();
            line.PurchaseInvoiceId = invoice.Id;
            await lineRepository.AddAsync(line, cancellationToken).ConfigureAwait(false);
        }

        PurchaseInvoiceCalculator.Compute(invoice, lines);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return invoice.Id;
    }
}
