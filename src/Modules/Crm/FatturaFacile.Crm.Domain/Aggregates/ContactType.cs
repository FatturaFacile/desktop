// <copyright file="ContactType.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Crm.Domain.Aggregates;

/// <summary>
/// Specifies the business relationship classification for a CRM contact.
/// </summary>
public enum ContactType
{
    /// <summary>
    /// Purchasing client or customer.
    /// </summary>
    Customer = 1,

    /// <summary>
    /// Vendor or goods/services supplier.
    /// </summary>
    Supplier = 2,

    /// <summary>
    /// Prospective business lead.
    /// </summary>
    Lead = 3,
}
