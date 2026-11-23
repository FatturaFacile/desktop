// <copyright file="WithholdingCertificateStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// The state of a Certificazione Unica.
/// </summary>
public enum WithholdingCertificateStatus
{
    /// <summary>
    /// Built from the withholdings of the year; figures may still change.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// The PDF or the telematic file was produced; the certificate is frozen.
    /// </summary>
    Issued = 1,
}
