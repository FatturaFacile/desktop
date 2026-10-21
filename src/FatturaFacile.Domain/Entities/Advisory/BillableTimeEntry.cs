// <copyright file="BillableTimeEntry.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Advisory;

/// <summary>
/// Represents billable time entry.
/// </summary>
public sealed class BillableTimeEntry : Entity<Guid>, ITenantEntity
{
    private BillableTimeEntry()
    {
    }

    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the client matter id.
    /// </summary>
    public Guid ClientMatterId { get; private set; }

    /// <summary>
    /// Gets the professional user id.
    /// </summary>
    public Guid ProfessionalUserId { get; private set; }

    /// <summary>
    /// Gets the activity date.
    /// </summary>
    public DateTime ActivityDate { get; private set; }

    /// <summary>
    /// Gets the minutes spent.
    /// </summary>
    public int MinutesSpent { get; private set; }

    /// <summary>
    /// Gets the hourly rate.
    /// </summary>
    public decimal HourlyRate { get; private set; }

    /// <summary>
    /// Gets the currency.
    /// </summary>
    public Currency Currency { get; private set; } = Currency.EUR;

    /// <summary>
    /// Gets the description.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether invoiced.
    /// </summary>
    public bool IsInvoiced { get; private set; }

    /// <summary>
    /// Gets the total fee.
    /// </summary>
    public decimal TotalFee => decimal.Round(
        MinutesSpent / 60m * HourlyRate,
        2,
        MidpointRounding.AwayFromZero);

    /// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="matterId">The matter id.</param>
    /// <param name="userId">The user id.</param>
    /// <param name="date">The date.</param>
    /// <param name="minutes">The minutes.</param>
    /// <param name="rate">The rate.</param>
    /// <param name="currency">The currency.</param>
    /// <param name="description">The description.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static BillableTimeEntry Create(
        Guid tenantId,
        Guid matterId,
        Guid userId,
        DateTime date,
        int minutes,
        decimal rate,
        Currency currency,
        string description)
    {
        return new BillableTimeEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ClientMatterId = matterId,
            ProfessionalUserId = userId,
            ActivityDate = date,
            MinutesSpent = minutes,
            HourlyRate = rate,
            Currency = currency,
            Description = description?.Trim() ?? string.Empty,
            IsInvoiced = false,
        };
    }

    /// <summary>
    /// Mark Invoiced.
    /// </summary>
    public void MarkInvoiced() => IsInvoiced = true;
}
