// <copyright file="BankReconciler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Accounting;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Banking;

/// <summary>
/// Reconciles bank movements with customer invoices (QR / creditor reference or invoice number) and supplier invoices
/// (reference, or IBAN and open amount), records the payment and posts it to the general ledger.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="BankReconciler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="journal">The journal writer.</param>
public sealed class BankReconciler(IUnitOfWork unitOfWork, JournalWriter journal)
{
    /// <summary>
    /// The document type of customer invoices.
    /// </summary>
    public const string CustomerInvoice = "Invoice";

    /// <summary>
    /// The document type of supplier invoices.
    /// </summary>
    public const string SupplierInvoice = "PurchaseInvoice";

    /// <summary>
    /// Tries to match a movement automatically (not saved).
    /// </summary>
    /// <param name="transaction">The movement.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True when matched.</returns>
    public Task<bool> TryAutoMatchAsync(BankTransaction transaction, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        return TryAutoMatchInternalAsync(transaction, cancellationToken);
    }

    /// <summary>
    /// Records a movement as payment of a document and posts it (not saved).
    /// </summary>
    /// <param name="transaction">The movement.</param>
    /// <param name="documentType">Invoice or PurchaseInvoice.</param>
    /// <param name="documentId">The document identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True when applied.</returns>
    /// <exception cref="InvalidOperationException">When the movement is already reconciled.</exception>
    public Task<bool> ApplyAsync(BankTransaction transaction, string documentType, Guid documentId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        return string.Equals(transaction.Status, "Matched", StringComparison.Ordinal)
            ? throw new InvalidOperationException("Il movimento è già riconciliato.")
            : ApplyInternalAsync(transaction, documentType, documentId, cancellationToken);
    }

    private async Task<bool> TryAutoMatchInternalAsync(BankTransaction transaction, CancellationToken cancellationToken)
    {
        var reference = PaymentReferenceNormalizer.Normalize(transaction.Reference);

        if (transaction.Amount > 0m)
        {
            var invoice = await FindCustomerInvoiceAsync(reference, transaction.Description, cancellationToken).ConfigureAwait(false);
            return invoice is not null && await ApplyAsync(transaction, CustomerInvoice, invoice.Id, cancellationToken).ConfigureAwait(false);
        }

        var supplierInvoices = await unitOfWork.Repository<PurchaseInvoice>().FindAsync(static _ => true, cancellationToken).ConfigureAwait(false);
        var open = supplierInvoices.Where(static p => string.Equals(p.Status, "Posted", StringComparison.Ordinal)).ToList();
        var supplierInvoice = open.FirstOrDefault(p => reference is not null && string.Equals(PaymentReferenceNormalizer.Normalize(p.PaymentReference), reference, StringComparison.Ordinal))
            ?? open.FirstOrDefault(p => p.TotalGross - p.PaidAmount == -transaction.Amount
                && string.Equals(PaymentReferenceNormalizer.Normalize(p.PaymentIban), PaymentReferenceNormalizer.Normalize(transaction.CounterpartyIban), StringComparison.Ordinal));

        return supplierInvoice is not null && await ApplyAsync(transaction, SupplierInvoice, supplierInvoice.Id, cancellationToken).ConfigureAwait(false);
    }

    private async Task<Invoice?> FindCustomerInvoiceAsync(string? reference, string? description, CancellationToken cancellationToken)
    {
        if (reference is not null)
        {
            var qrBills = await unitOfWork.Repository<InvoiceQrBillData>().FindAsync(static _ => true, cancellationToken).ConfigureAwait(false);
            var qrBill = qrBills.FirstOrDefault(q => string.Equals(PaymentReferenceNormalizer.Normalize(q.PaymentReference), reference, StringComparison.Ordinal));

            if (qrBill is not null)
            {
                return await unitOfWork.Repository<Invoice>().GetByIdAsync(qrBill.InvoiceId, cancellationToken).ConfigureAwait(false);
            }
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        var invoices = await unitOfWork.Repository<Invoice>().FindAsync(static _ => true, cancellationToken).ConfigureAwait(false);
        return invoices.FirstOrDefault(i => !string.Equals(i.PaymentStatus, "paid", StringComparison.Ordinal)
            && !string.IsNullOrWhiteSpace(i.InvoiceNumber)
            && description.Contains(i.InvoiceNumber, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<bool> ApplyInternalAsync(BankTransaction transaction, string documentType, Guid documentId, CancellationToken cancellationToken)
    {
        var account = await unitOfWork.Repository<BankAccount>().GetByIdAsync(transaction.BankAccountId, cancellationToken).ConfigureAwait(false);
        var bank = account?.LedgerAccountId ?? await journal.AccountAsync(StandardAccount.Bank, cancellationToken).ConfigureAwait(false);
        var amount = Math.Abs(transaction.Amount);

        if (string.Equals(documentType, CustomerInvoice, StringComparison.Ordinal))
        {
            var invoice = await unitOfWork.Repository<Invoice>().GetByIdAsync(documentId, cancellationToken).ConfigureAwait(false);

            if (invoice is null)
            {
                return false;
            }

            invoice.TotalPaid += amount;
            var due = invoice.ReceivableAmount > 0m ? invoice.ReceivableAmount : invoice.TotalGross;
            invoice.PaymentStatus = invoice.TotalPaid >= due ? "paid" : "partial";
            unitOfWork.Repository<Invoice>().Update(invoice);
            var receivables = await journal.AccountAsync(StandardAccount.Receivables, cancellationToken).ConfigureAwait(false);
            _ = await journal.WriteAsync("INC", transaction.BookingDate, $"Incasso fattura {invoice.InvoiceNumber}", invoice.Id, [(bank, amount, 0m), (receivables, 0m, amount)], cancellationToken).ConfigureAwait(false);
        }
        else
        {
            var invoice = await unitOfWork.Repository<PurchaseInvoice>().GetByIdAsync(documentId, cancellationToken).ConfigureAwait(false);

            if (invoice is null)
            {
                return false;
            }

            invoice.PaidAmount += amount;
            invoice.Status = invoice.PaidAmount >= invoice.TotalGross ? "Paid" : invoice.Status;
            unitOfWork.Repository<PurchaseInvoice>().Update(invoice);
            var payables = await journal.AccountAsync(StandardAccount.Payables, cancellationToken).ConfigureAwait(false);
            _ = await journal.WriteAsync("PAG", transaction.BookingDate, $"Pagamento fattura fornitore {invoice.DocumentNumber}", invoice.Id, [(payables, amount, 0m), (bank, 0m, amount)], cancellationToken).ConfigureAwait(false);
        }

        transaction.Status = "Matched";
        transaction.MatchedDocumentType = documentType;
        transaction.MatchedDocumentId = documentId;
        return true;
    }
}
