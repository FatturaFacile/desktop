// <copyright file="ImportPurchaseInvoiceFileCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Purchasing;
using MediatR;

namespace FatturaFacile.Application.Features.Purchasing;

/// <summary>
/// Handles <see cref="ImportPurchaseInvoiceFileCommand"/>.
/// </summary>
/// <param name="importer">The importer.</param>
public sealed class ImportPurchaseInvoiceFileCommandHandler(PurchaseInvoiceImporter importer) : IRequestHandler<ImportPurchaseInvoiceFileCommand, PurchaseImportResult>
{
    /// <inheritdoc />
    public Task<PurchaseImportResult> Handle(ImportPurchaseInvoiceFileCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return importer.ImportAsync(request.FileName, request.Content, cancellationToken);
    }
}
