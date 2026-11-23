// <copyright file="DetectBezugsteuerCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SwissVat;
using MediatR;

namespace FatturaFacile.Application.Features.SwissVat;

/// <summary>
/// Handles <see cref="DetectBezugsteuerCommand"/>.
/// </summary>
public sealed class DetectBezugsteuerCommandHandler : IRequestHandler<DetectBezugsteuerCommand, BezugsteuerEntryDto?>
{
    /// <inheritdoc/>
    public Task<BezugsteuerEntryDto?> Handle(DetectBezugsteuerCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entry = BezugsteuerDetector.Evaluate(
            request.SupplierName,
            request.SupplierCountry,
            request.InvoiceNumber,
            request.InvoiceDate,
            request.Description,
            request.NetAmountChf,
            request.ChargedSwissVatAmount,
            request.Method);

        return Task.FromResult(entry);
    }
}
