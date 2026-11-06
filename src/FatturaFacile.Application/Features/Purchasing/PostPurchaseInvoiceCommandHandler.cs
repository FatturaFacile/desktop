// <copyright file="PostPurchaseInvoiceCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Accounting;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Purchasing;

/// <summary>
/// Handler for <see cref="PostPurchaseInvoiceCommand"/>: expenses (with the non-deductible VAT) and deductible input VAT
/// in debit, supplier payables in credit.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PostPurchaseInvoiceCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="journal">The journal writer.</param>
public sealed class PostPurchaseInvoiceCommandHandler(IUnitOfWork unitOfWork, JournalWriter journal) : IRequestHandler<PostPurchaseInvoiceCommand, bool>
{
    /// <inheritdoc/>
    public Task<bool> Handle(PostPurchaseInvoiceCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.Id, cancellationToken);
    }

    private async Task<bool> HandleInternalAsync(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await unitOfWork.Repository<PurchaseInvoice>().GetByIdAsync(id, cancellationToken).ConfigureAwait(false);

        if (invoice is null || !string.Equals(invoice.Status, "Draft", StringComparison.Ordinal))
        {
            return false;
        }

        var lines = await unitOfWork.Repository<PurchaseInvoiceLine>().FindAsync(l => l.PurchaseInvoiceId == id, cancellationToken).ConfigureAwait(false);
        var defaultExpense = await journal.AccountAsync(StandardAccount.Purchases, cancellationToken).ConfigureAwait(false);
        var inputVat = await journal.AccountAsync(StandardAccount.InputVat, cancellationToken).ConfigureAwait(false);
        var payables = await journal.AccountAsync(StandardAccount.Payables, cancellationToken).ConfigureAwait(false);

        List<(Guid AccountId, decimal Debit, decimal Credit)> postings = [];

        foreach (var group in lines.GroupBy(l => l.AccountId ?? defaultExpense))
        {
            var deductible = group.Sum(static l => Math.Round(l.VatAmount * l.DeductiblePercent / 100m, 2, MidpointRounding.AwayFromZero));
            var expense = group.Sum(static l => l.NetAmount + l.VatAmount) - deductible;
            postings.Add((group.Key, expense, 0m));
        }

        postings.Add((inputVat, invoice.DeductibleVat, 0m));
        postings.Add((payables, 0m, invoice.TotalGross));

        var entry = await journal.WriteAsync("AC", invoice.RegistrationDate, $"Fattura fornitore {invoice.DocumentNumber}", invoice.Id, postings, cancellationToken).ConfigureAwait(false);
        invoice.Status = "Posted";
        invoice.JournalEntryId = entry.Id;
        unitOfWork.Repository<PurchaseInvoice>().Update(invoice);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
