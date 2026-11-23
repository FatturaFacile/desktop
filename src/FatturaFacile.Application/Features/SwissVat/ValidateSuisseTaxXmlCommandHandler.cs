// <copyright file="ValidateSuisseTaxXmlCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SwissVat;
using MediatR;

namespace FatturaFacile.Application.Features.SwissVat;

/// <summary>
/// Handles <see cref="ValidateSuisseTaxXmlCommand"/> by building eCH-0217 XML and validating it against XSD.
/// </summary>
public sealed class ValidateSuisseTaxXmlCommandHandler
    : IRequestHandler<ValidateSuisseTaxXmlCommand, ValidateSuisseTaxXmlOutcome>
{
    /// <inheritdoc/>
    public Task<ValidateSuisseTaxXmlOutcome> Handle(
        ValidateSuisseTaxXmlCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var quadratura = SwissVatQuadraturaValidator.Validate(
            request.Result,
            request.UidMwst,
            request.PendingBezugsteuerCount);
        var xml = SuisseTaxXmlBuilder.Build(
            request.Result,
            request.UidMwst,
            request.CompanyName,
            quadratura.Passed);
        var validation = SuisseTaxXsdValidator.ValidateAll(xml, request.UidMwst, quadratura);

        return Task.FromResult(new ValidateSuisseTaxXmlOutcome(xml, validation));
    }
}
