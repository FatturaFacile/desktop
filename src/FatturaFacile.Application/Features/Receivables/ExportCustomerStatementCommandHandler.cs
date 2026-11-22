// <copyright file="ExportCustomerStatementCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Handler for <see cref="ExportCustomerStatementCommand"/>.
/// </summary>
/// <param name="service">The receivables service.</param>
/// <param name="saver">The file saver.</param>
public sealed class ExportCustomerStatementCommandHandler(IReceivablesService service, IReceivablesFileSaver saver) : IRequestHandler<ExportCustomerStatementCommand, ReceivablesExportResult>
{
    /// <inheritdoc/>
    public async Task<ReceivablesExportResult> Handle(ExportCustomerStatementCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var file = await service.ExportStatementAsync(request.CustomerId, request.Format, request.From, request.To, request.Currency, cancellationToken).ConfigureAwait(false);
        if (file is null)
        {
            return new ReceivablesExportResult(null, Offline: true);
        }

        var path = await saver.SaveAsync(file, cancellationToken).ConfigureAwait(false);

        return new ReceivablesExportResult(path, Offline: false);
    }
}
