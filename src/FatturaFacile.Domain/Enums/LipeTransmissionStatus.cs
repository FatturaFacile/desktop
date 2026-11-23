// <copyright file="LipeTransmissionStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// The lifecycle of a LIPE communication, from the generated file to the receipt of the tax authority.
/// </summary>
public enum LipeTransmissionStatus
{
    /// <summary>
    /// Generated, not yet validated against the schema.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Validated against the structural XSD profile and the ministerial rules.
    /// </summary>
    Validated = 1,

    /// <summary>
    /// A signed (CAdES / P7M) copy of the same content was attached.
    /// </summary>
    SignatureVerified = 2,

    /// <summary>
    /// Sent through Entratel or Desktop Telematico.
    /// </summary>
    Transmitted = 3,

    /// <summary>
    /// The receipt of the tax authority accepted the file.
    /// </summary>
    Accepted = 4,

    /// <summary>
    /// The receipt of the tax authority rejected the file.
    /// </summary>
    Rejected = 5,
}
