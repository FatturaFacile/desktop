// <copyright file="VatRegisterSection.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// A sezionale: a separately numbered series inside one VAT register.
/// </summary>
public enum VatRegisterSection
{
    /// <summary>
    /// Ordinary documents.
    /// </summary>
    Ordinary = 0,

    /// <summary>
    /// Deferred invoices (fatture differite, art. 21 c. 4 lett. a).
    /// </summary>
    Deferred = 1,

    /// <summary>
    /// Credit notes (note di variazione in diminuzione).
    /// </summary>
    CreditNote = 2,

    /// <summary>
    /// Debit notes (note di variazione in aumento).
    /// </summary>
    DebitNote = 3,

    /// <summary>
    /// Self-invoices and integrated reverse-charge documents (autofatture).
    /// </summary>
    SelfInvoice = 4,
}
