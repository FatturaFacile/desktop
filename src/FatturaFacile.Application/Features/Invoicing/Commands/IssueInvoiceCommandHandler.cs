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
