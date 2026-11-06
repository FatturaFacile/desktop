// <copyright file="ContactType.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Relationship type for a business partner or contact.
/// </summary>
public enum ContactType
{
    /// <summary>
    /// Customer (cliente).
    /// </summary>
    Client = 0,

    /// <summary>
    /// Supplier or vendor (fornitore).
    /// </summary>
    Supplier = 1,

    /// <summary>
    /// Both customer and supplier.
    /// </summary>
    Both = 2,
}
