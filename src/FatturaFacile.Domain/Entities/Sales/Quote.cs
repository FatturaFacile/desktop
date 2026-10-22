// <copyright file="Quote.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Sales;

/// <summary>
/// Represents a commercial sales quotation aggregate submitted to a client, detailing estimated amounts,
/// validity expiry, and client acceptance status prior to order or invoice creation.
/// </summary>
public sealed class Quote : AggregateRoot<Guid>, ITenantEntity
{

    /// <summary>
    /// Initializes a new instance of the <see cref="Quote"/> class for entity framework or serialization.
    /// </summary>
    private Quote()
    {
    }

    /// <summary>
    /// Gets or sets the unique tenant identifier for multi-tenant data partitioning.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique commercial quotation number (e.g., "PREV-2026/042").
    /// </summary>
    public string QuoteNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the unique identifier of the target customer/client receiving this quotation.
    /// </summary>
    public Guid ClientId { get; private set; }

    /// <summary>
    /// Gets the expiration date until which the quoted commercial terms and pricing remain contractually valid.
    /// </summary>
    public DateTime ExpiryDate { get; private set; }

    /// <summary>
    /// Gets the monetary total value of the quote, including currency and nominal amount.
    /// </summary>
    public Money TotalAmount { get; private set; } = Money.Euro(amount: 0);

    /// <summary>
    /// Gets a value indicating whether the prospective client has formally accepted this quotation.
    /// </summary>
    public bool IsAccepted { get; private set; }

    /// <summary>
    /// Creates a new commercial <see cref="Quote"/> aggregate in an initial unaccepted state.
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <param name="quoteNumber">The formal reference number assigned to the quotation.</param>
    /// <param name="clientId">The client entity identifier.</param>
    /// <param name="expiry">The date until which the quotation prices remain binding.</param>
    /// <param name="total">The monetary total value of the quote.</param>
    /// <returns>A newly instantiated <see cref="Quote"/> aggregate.</returns>
    public static Quote Create(
        Guid     tenantId,
        string   quoteNumber,
        Guid     clientId,
        DateTime expiry,
        Money    total)
    {
        return new Quote
               {
                   Id          = Guid.NewGuid(),
                   TenantId    = tenantId,
                   QuoteNumber = quoteNumber,
                   ClientId    = clientId,
                   ExpiryDate  = expiry,
                   TotalAmount = total,
                   IsAccepted  = false,
               };
    }

}
