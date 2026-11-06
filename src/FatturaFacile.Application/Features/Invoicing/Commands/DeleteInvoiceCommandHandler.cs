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
/// <remarks>
/// Initializes a new instance of the <see cref="DeleteInvoiceCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class DeleteInvoiceCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteInvoiceCommand, bool>
{
    /// <inheritdoc/>
    public Task<bool> Handle(DeleteInvoiceCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<bool> HandleInternalAsync(
        DeleteInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        var invoice = await unitOfWork.Repository<Invoice>()
           .GetByIdAsync(request.InvoiceId, cancellationToken).ConfigureAwait(false);

        if (invoice is null || !string.Equals(invoice.Status, "draft", StringComparison.Ordinal))
        {
            return false;
        }

        unitOfWork.Repository<Invoice>().Delete(invoice);

        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
}
