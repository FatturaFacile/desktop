// <copyright file="CalculateSwissVatReturnCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SwissVat;
using MediatR;

namespace FatturaFacile.Application.Features.SwissVat;

/// <summary>
/// Handles <see cref="CalculateSwissVatReturnCommand"/> by running <see cref="SwissVatCalculator"/>
/// and <see cref="SwissVatQuadraturaValidator"/>.
/// </summary>
public sealed class CalculateSwissVatReturnCommandHandler
    : IRequestHandler<CalculateSwissVatReturnCommand, CalculateSwissVatReturnOutcome>
{
    /// <inheritdoc/>
    public Task<CalculateSwissVatReturnOutcome> Handle(
        CalculateSwissVatReturnCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = SwissVatCalculator.Calculate(request.Input);
        var quadratura = SwissVatQuadraturaValidator.Validate(
            result,
            request.UidMwst,
            request.PendingBezugsteuerCount);

        return Task.FromResult(new CalculateSwissVatReturnOutcome(result, quadratura));
    }
}
