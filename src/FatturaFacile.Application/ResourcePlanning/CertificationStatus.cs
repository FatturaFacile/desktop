// <copyright file="CertificationStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// The state of the certificate a skill may require.
/// </summary>
[JsonConverter(typeof(SnakeCaseEnumJsonConverter<CertificationStatus>))]
public enum CertificationStatus
{
    /// <summary>
    /// The skill needs no certificate.
    /// </summary>
    NotRequired,

    /// <summary>
    /// The certificate is valid.
    /// </summary>
    Valid,

    /// <summary>
    /// The certificate is missing.
    /// </summary>
    Missing,

    /// <summary>
    /// The certificate has expired.
    /// </summary>
    Expired,
}
