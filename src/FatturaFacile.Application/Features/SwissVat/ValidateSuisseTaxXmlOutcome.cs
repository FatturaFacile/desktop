// <copyright file="ValidateSuisseTaxXmlOutcome.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SwissVat;

namespace FatturaFacile.Application.Features.SwissVat;

/// <summary>
/// Result of SuisseTax eCH-0217 XML generation and XSD/quadratura validation.
/// </summary>
/// <param name="XmlPayload">Generated XML payload.</param>
/// <param name="Validation">Validation report.</param>
public sealed record ValidateSuisseTaxXmlOutcome(
    string XmlPayload,
    SuisseTaxValidationResultDto Validation);
