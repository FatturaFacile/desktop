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
public sealed class IssueInvoiceCommandHandler : IRequestHandler<IssueInvoiceCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="IssueInvoiceCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    public IssueInvoiceCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc/>
    public async Task<bool> Handle(IssueInvoiceCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var invoice = await _unitOfWork.Repository<Invoice>().GetByIdAsync(request.InvoiceId, cancellationToken).ConfigureAwait(false);
        if (invoice == null)
        {
            return false;
        }

        invoice.Status = "issued";
        invoice.PostedAt = DateTime.UtcNow;
        _unitOfWork.Repository<Invoice>().Update(invoice);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
}
