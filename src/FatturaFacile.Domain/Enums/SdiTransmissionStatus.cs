// <copyright file="SdiTransmissionStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Specifies sdi transmission status.
/// </summary>
public enum SdiTransmissionStatus
{
    /// <summary>
    /// Draft.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Ready To Send.
    /// </summary>
    ReadyToSend = 1,

    /// <summary>
    /// Sent.
    /// </summary>
    Sent = 2,

    /// <summary>
    /// Delivered.
    /// </summary>
    Delivered = 3,

    /// <summary>
    /// Failed.
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Rejected.
    /// </summary>
    Rejected = 5,
}
