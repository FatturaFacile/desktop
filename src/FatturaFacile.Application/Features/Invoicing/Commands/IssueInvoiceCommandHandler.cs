// <copyright file="IssueInvoiceCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
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
