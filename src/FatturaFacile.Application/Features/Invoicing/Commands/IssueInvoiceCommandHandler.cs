// <copyright file="IssueInvoiceCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Invoicing.Commands;

/// <summary>
/// Handler for <see cref="IssueInvoiceCommand"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="IssueInvoiceCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class IssueInvoiceCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<IssueInvoiceCommand, bool>
{
    /// <inheritdoc/>
    public Task<bool> Handle(
        IssueInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(
            request,
            cancellationToken);
    }

    /// <summary>
    /// Enforces the plafond of the declarations of intent (art. 8 c. 1 lett. c DPR 633/1972): an Italian invoice with lines
    /// under nature N3.5 is issued only while a valid letter of the customer covers it, and the erosion is recorded.
    /// </summary>
    /// <param name="invoice">The invoice being issued.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="InvalidOperationException">When no valid letter covers the invoice or its plafond would be exceeded.</exception>
    private async Task ApplyIntentLetterPlafondAsync(
        Invoice invoice,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(invoice.Jurisdiction, "IT", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var invoiceId = invoice.Id;
        var items = await unitOfWork
           .Repository<InvoiceItem>()
           .FindAsync(i => i.InvoiceId == invoiceId, cancellationToken)
           .ConfigureAwait(false);
        var amount = items
           .Where(static i => string.Equals(i.VatNature, IntentLetterPlafond.Nature, StringComparison.Ordinal))
           .Sum(static i => i.LineTotal);

        if (amount <= 0m)
        {
            return;
        }

        var (letters, usage) = await IntentLetterLookup.LoadAsync(unitOfWork, cancellationToken).ConfigureAwait(false);
        var check = IntentLetterPlafond.Evaluate(letters, usage, invoice.ContactId, invoice.Date, amount);

        if (!check.Allowed || check.LetterId is not { } letterId)
        {
            throw new InvalidOperationException(check.BlockCode ?? VatComplianceErrors.BlockNoValidLetter);
        }

        await unitOfWork
           .Repository<IntentLetterUsage>()
           .AddAsync(
                new IntentLetterUsage { IntentLetterId = letterId, InvoiceId = invoiceId, Amount = amount, UsedAt = invoice.Date.Date },
                cancellationToken)
           .ConfigureAwait(false);
    }

    private async Task<bool> HandleInternalAsync(
        IssueInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        var invoice = await unitOfWork
           .Repository<Invoice>()
           .GetByIdAsync(
                request.InvoiceId,
                cancellationToken)
           .ConfigureAwait(false);

        if (invoice is null)
        {
            return false;
        }

        if (!string.Equals(invoice.Status, "draft", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"La fattura {invoice.InvoiceNumber} è già stata emessa: un documento emesso non può essere riemesso o modificato (usare una nota di credito).");
        }

        await ApplyIntentLetterPlafondAsync(invoice, cancellationToken).ConfigureAwait(false);

        var qrBills = await unitOfWork
           .Repository<InvoiceQrBillData>()
           .FindAsync(q => q.InvoiceId == invoice.Id, cancellationToken)
           .ConfigureAwait(false);

        var sdiFiles = await unitOfWork
           .Repository<InvoiceSdiMetadata>()
           .FindAsync(s => s.InvoiceId == invoice.Id, cancellationToken)
           .ConfigureAwait(false);

        var complianceErrors = qrBills
           .Select(static q => q.ValidationErrors)
           .Concat(sdiFiles.Select(static s => s.ValidationErrors))
           .Where(static e => !string.IsNullOrWhiteSpace(e))
           .ToList();

        if (complianceErrors.Count > 0)
        {
            throw new InvalidOperationException(
                "La fattura non è conforme e non può essere emessa:" + Environment.NewLine + string.Join(Environment.NewLine, complianceErrors));
        }

        invoice.Status = "issued";
        invoice.PostedAt = DateTime.UtcNow;

        unitOfWork
           .Repository<Invoice>()
           .Update(invoice);

        _ = await unitOfWork
           .SaveChangesAsync(cancellationToken)
           .ConfigureAwait(false);

        return true;
    }
}
