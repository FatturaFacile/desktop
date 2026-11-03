// <copyright file="BillableTimeEntry.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Advisory;

/// <summary>
/// Represents a billable or non-billable time entry recorded by a professional advisor
/// against an active client engagement or advisory matter.
/// </summary>
public sealed class BillableTimeEntry : Entity<Guid>, ITenantEntity
{
    private const decimal MinutesPerHour        = 60m;
    private const int     CurrencyDecimalPlaces = 2;

    /// <summary>
    /// Initializes a new instance of the <see cref="BillableTimeEntry"/> class for entity framework or serialization.
    /// </summary>
    private BillableTimeEntry()
    {
    }

    /// <summary>
    /// Gets or sets the unique identifier of the tenant organization owning this time entry.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique identifier of the client legal, tax, or corporate matter to which this time entry is allocated.
    /// </summary>
    public Guid ClientMatterId { get; private set; }

    /// <summary>
    /// Gets the unique identifier of the professional user or fee earner who performed the advisory activity.
    /// </summary>
    public Guid ProfessionalUserId { get; private set; }

    /// <summary>
    /// Gets the calendar date on which the advisory work or consultation was performed.
    /// </summary>
    public DateTime ActivityDate { get; private set; }

    /// <summary>
    /// Gets the duration of the advisory activity recorded in minutes.
    /// </summary>
    public int MinutesSpent { get; private set; }

    /// <summary>
    /// Gets the agreed hourly billing rate applied to this entry.
    /// </summary>
    public decimal HourlyRate { get; private set; }

    /// <summary>
    /// Gets the currency denomination used for the hourly billing rate and fee calculations.
    /// </summary>
    public Currency Currency { get; private set; } = Currency.EUR;

    /// <summary>
    /// Gets the narrative summary detailing the nature and scope of the professional work performed.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether this time entry has been settled or billed in an issued fee note.
    /// </summary>
    public bool IsInvoiced { get; private set; }

    /// <summary>
    /// Gets the computed total fee for this activity, calculated by applying the hourly rate to elapsed minutes.
    /// </summary>
    public decimal TotalFee => decimal.Round(
                                             d: MinutesSpent / MinutesPerHour * HourlyRate,
                                             decimals: CurrencyDecimalPlaces,
                                             mode: MidpointRounding.AwayFromZero);

    /// <summary>
    /// Creates a new unbilled advisory time entry associated with a designated client matter.
    /// </summary>
    /// <param name="data">The parameter object containing billable time entry details.</param>
    /// <returns>A newly initialized, unbilled <see cref="BillableTimeEntry"/> instance.</returns>
    public static BillableTimeEntry Create(
        BillableTimeEntryData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return new BillableTimeEntry
        {
            Id                 = Guid.NewGuid(),
            TenantId           = data.TenantId,
            ClientMatterId     = data.MatterId,
            ProfessionalUserId = data.UserId,
            ActivityDate       = data.Date,
            MinutesSpent       = data.Minutes,
            HourlyRate         = data.Rate,
            Currency           = data.Currency,
            Description        = data.Description?.Trim() ?? string.Empty,
            IsInvoiced         = false,
        };
    }

    /// <summary>
    /// Marks this time entry as invoiced to prevent subsequent double-billing in future fee notes.
    /// </summary>
    public void MarkInvoiced() => IsInvoiced = true;

}
