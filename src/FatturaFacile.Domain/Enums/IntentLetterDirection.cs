// <copyright file="IntentLetterDirection.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Whether the company received the declaration of intent from a customer or issued it to a supplier.
/// </summary>
public enum IntentLetterDirection
{
    /// <summary>
    /// Received from a customer (habitual exporter): the company may invoice under art. 8 c. 1 lett. c DPR 633/1972.
    /// </summary>
    Received = 0,

    /// <summary>
    /// Issued to a supplier: the company is the habitual exporter.
    /// </summary>
    Issued = 1,
}
