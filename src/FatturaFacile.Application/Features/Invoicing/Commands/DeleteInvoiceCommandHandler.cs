// <copyright file="DeleteInvoiceCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Invoicing.Commands;

/// <summary>
/// Handler for <see cref="DeleteInvoiceCommand"/>.
/// </summary>
public sealed class DeleteInvoiceCommandHandler : IRequestHandler<DeleteInvoiceCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteInvoiceCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    public DeleteInvoiceCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc/>
    public async Task<bool> Handle(DeleteInvoiceCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var invoice = await _unitOfWork.Repository<Invoice>().GetByIdAsync(request.InvoiceId, cancellationToken).ConfigureAwait(false);
        if (invoice == null || !string.Equals(invoice.Status, "draft", StringComparison.Ordinal))
        {
            return false;
        }

        _unitOfWork.Repository<Invoice>().Delete(invoice);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
