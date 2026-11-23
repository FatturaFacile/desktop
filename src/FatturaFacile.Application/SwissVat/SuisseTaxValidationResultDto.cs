// <copyright file="SuisseTaxValidationResultDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SwissVat;

/// <summary>
/// Outcome of pre-export SuisseTax eCH-0217 XSD and business validation.
/// </summary>
/// <param name="Valid">True when XSD, business rules, and quadratura all pass.</param>
/// <param name="XsdValid">True when the XML satisfies the eCH-0217 v2.0 XSD schema.</param>
/// <param name="QuadraturaPassed">True when quadratura has zero blockers.</param>
/// <param name="SchemaVersion">Schema identifier.</param>
/// <param name="Errors">All validation error messages.</param>
public sealed record SuisseTaxValidationResultDto(
    bool Valid,
    bool XsdValid,
    bool QuadraturaPassed,
    string SchemaVersion,
    IReadOnlyList<string> Errors);
