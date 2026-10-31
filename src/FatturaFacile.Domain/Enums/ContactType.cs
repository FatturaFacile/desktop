// <copyright file="ContactType.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Distinguishes between commercial counterpart types within the tenant contact book.
/// </summary>
public enum ContactType
{
    /// <summary>
    /// Commercial debtor, client, or customer buying goods or professional services.
    /// </summary>
    Customer = 0,

    /// <summary>
    /// Commercial creditor, vendor, or supplier delivering supplies, merchandise, or services.
    /// </summary>
    Supplier = 1,
}
